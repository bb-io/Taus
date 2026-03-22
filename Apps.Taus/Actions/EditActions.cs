using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Estimate;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Apps.Taus.Models.TausApiResponseDtos;
using Apps.Taus.Models.XliffBatch;
using Apps.Taus.Services.XliffBatch;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;
using Blackbird.Filters.Xliff.Xliff2;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Text;
using Segment = Blackbird.Filters.Transformations.Segment;

namespace Apps.Taus.Actions;

[ActionList("Editing")]
public class EditActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : TausInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.EditFile)]
    [Action("Edit", Description = "Edit translated content and output updated segments with quality metadata.")]
    public async Task<ContentEditResponse> EditContent([ActionParameter] EditContentRequest input)
    {
        var stream = await fileManagementClient.DownloadAsync(input.File);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var bytes = memoryStream.ToArray();
        var contentString = System.Text.Encoding.UTF8.GetString(bytes);

        var content = Transformation.Parse(contentString, input.File.Name);
        if (content == null)
        {
            throw new PluginApplicationException(
                "Something went wrong parsing this XLIFF file. Please send a copy of this file to the team for inspection!");
        }

        if (content.SourceLanguage == null)
        {
            throw new PluginMisconfigurationException(
                "The source language is not defined yet. Please assign the source language in this action.");
        }

        if (content.TargetLanguage == null)
        {
            throw new PluginMisconfigurationException(
                "The target language is not defined yet. Please assign the target language in this action.");
        }

        var srcLanguage = content.SourceLanguage;
        var trgLanguage = input.TargetLanguage ?? content.TargetLanguage;

        var processedSegmentsCount = 0;
        var finalizedSegmentsCount = 0;
        var riskySegmentsCount = 0;
        int billedCharacters = 0;
        int billedWords = 0;
        int updatedCount = 0;

        async Task<IEnumerable<EstimateOutput>> BatchProcess(IEnumerable<(Unit Unit, Segment Segment)> batch)
        {
            var result = new List<EstimateOutput>();

            foreach (var segment in batch)
            {
                Task<EstimateOutput> EstimateAction() => Estimate(new EstimateInput
                {
                    Source = segment.Segment.GetSource(),
                    SourceLanguage = srcLanguage,
                    Target = segment.Segment.GetTarget(),
                    TargetLanguage = trgLanguage,
                    ApplyApe = true,
                    ApeLowThreshold = input.ApeLowThreshold ?? 0,
                    ApeThreshold = input.ApeThreshold ?? 1,
                    UseRag = input.UseRag,
                });

                var estimationResult = await EstimateAction();
                result.Add(estimationResult);

            }
            return result;
        }

        // When TAUS implements batching, this can be utilized better
        var units = await content.GetUnits().Batch(10, x => !x.IsIgnorbale && !x.IsInitial && x.State != SegmentState.Final).Process(BatchProcess);

        foreach (var (unit, results) in units)
        {
            float unitScore = 0;
            var localBilledCharacters = 0;
            var localBilledWords = 0;
            foreach (var (segment, result) in results)
            {
                billedCharacters += result.EstimateResult.BilledCharacters;
                localBilledCharacters += result.EstimateResult.BilledCharacters;
                var score = result.EstimateResult?.Score;
                if (score == null) continue;

                processedSegmentsCount++;

                if (result.ApeResult?.ApeRevisions.Count > 0)
                {
                    updatedCount++;
                    finalizedSegmentsCount++;
                    var revision = result.ApeResult.ApeRevisions.Last();
                    billedWords += result.ApeResult.BilledWords;
                    localBilledWords += result.ApeResult.BilledWords;
                    segment.SetTarget(revision.Translation);
                    segment.State = SegmentState.Final;
                    unit.Notes.Add(new Note(revision.Remarks) { Category = "epic-remark", Reference = segment.Id });
                    unitScore += result.ApeResult.Score;
                }
                else if (score >= input.Threshold)
                {
                    segment.State = SegmentState.Final;
                    finalizedSegmentsCount++;
                    unitScore += score.Value;
                }
                else
                {
                    riskySegmentsCount++;
                }
            }

            unit.Quality.ProfileReference = "https://api.taus.net/2.0/estimate";
            unit.Quality.ScoreThreshold = input.Threshold;
            unit.Quality.Score = unitScore / results.Count();
            unit.Provenance.Review.Tool = "TAUS APE";
            if (localBilledWords > 0)
            {
                unit.AddUsage("TAUS APE", localBilledWords, UsageUnit.Words);
            }
            if (localBilledCharacters > 0)
            {
                unit.AddUsage("TAUS QE", localBilledCharacters, UsageUnit.Characters);
            }
        }

        Stream streamResult;
        if (input.OutputFileHandling == "original")
        {
            if (Xliff1Serializer.IsXliff1(contentString))
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                streamResult = xliff1String.ToStream();
            }
            else if (Xliff2Serializer.IsXliff2(contentString))
            {
                var xliff2String = Xliff2Serializer.Serialize(content);
                streamResult = xliff2String.ToStream();
            }
            else
            {
                var targetContent = content.Target();
                streamResult = targetContent.Serialize().ToStream();
            }
        }
        else
        {
            streamResult = content.Serialize().ToStream();
        }

        var finalFile = await fileManagementClient.UploadAsync(streamResult, input.File.ContentType, input.File.Name);

        return new ContentEditResponse
        {
            File = finalFile,
            TotalSegmentsReviewed = processedSegmentsCount,
            TotalSegmentsUpdated = updatedCount,
            TotalSegmentsFinalized = finalizedSegmentsCount,
            TotalSegmentsUnderThreshhold = riskySegmentsCount,
            BilledCharacters = billedCharacters,
            BilledWords = billedWords,
        };
    }

    [BlueprintActionDefinition(BlueprintAction.EditText)]
    [Action("Edit text", Description = "Edit translated text and output an improved target text.")]
    public async Task<EditTextOutput> EditText([ActionParameter] EditTextRequest input)
    {
        var response = await Estimate(new EstimateInput
        {
            Source = input.SourceText,
            SourceLanguage = input.SourceLanguage,
            Target = input.TargetText,
            TargetLanguage = input.TargetLanguage,
            ApplyApe = true,
            ApeLowThreshold = input.ApeLowThreshold ?? 0,
            ApeThreshold = input.ApeThreshold ?? 1,
            UseRag = input.UseRag,
        });

        return new EditTextOutput(response, input.TargetText);
    }

    [Action("Edit in background", Description = "Edit translated content in background jobs and output job IDs for later retrieval.")]
    public async Task<ContentEditInBackgroundResponse> EditContentInBackground([ActionParameter] EditContentInBackgroundRequest input)
    {
        var jobIds = new List<string>();
        var transformationFileRefs = new List<FileReference>();
        var jobCreationErrors = new List<string>();
        var totalSegments = 0;
        var processedSegments = 0;

        foreach (var file in input.Files)
        {
            try
            {
                var (jobId, transformation, totalSegmentsInFile, processedSegmentsInFile) = await CreateEditBackgroundJob(file, input);
                jobIds.Add(jobId);
                transformationFileRefs.Add(transformation);
                totalSegments += totalSegmentsInFile;
                processedSegments += processedSegmentsInFile;
            }
            catch (Exception ex)
            {
                jobCreationErrors.Add($"{file.Name}: {ex.Message}");
            }
        }

        return new()
        {
            TausBackgroundJobIds = jobIds,
            TausTransformationFiles = transformationFileRefs,
            TausJobCreationErrors = jobCreationErrors,
            TotalSegments = totalSegments,
            ProcessedSegments = processedSegments,
        };
    }
    
    [Action("Download background files", Description = "Download files processed in background jobs and output updated files with usage details.")]
    public async Task<BackgroundContentResponse> DownloadContentFromBackground([ActionParameter] BackgroundDownloadRequest request)
    {
        var processedFiles = new List<BackgroundFileResult>();
        var totalBilledWords = 0;
        var totalBilledCharacters = 0;
        var errors = new List<string>();

        foreach (var transformationFileRef in request.TausTransformationFiles)
        {
            var transformationStream = await fileManagementClient.DownloadAsync(transformationFileRef);
            using var reader = new StreamReader(transformationStream);
            var contentString = await reader.ReadToEndAsync();
            var transformation = Transformation.Parse(contentString, transformationFileRef.Name);

            // Extract Job ID from metadata
            var expectedJobId = transformation.MetaData.Find(m => 
                m.Category.Contains(TransformationTausMetadata.Category)
                && m.Type == TransformationTausMetadata.JobIdType);

            if (string.IsNullOrWhiteSpace(expectedJobId?.Value))
            {
                errors.Add($"File {transformationFileRef.Name} does not contain the expected metadata to link it to a TAUS batch job.");
                continue;
            }

            try
            {
                var fileResult = await ApplyEdits(transformation, expectedJobId.Value, request);
                processedFiles.Add(fileResult);
                totalBilledWords += fileResult.BilledWords;
                totalBilledCharacters += fileResult.BilledCharacters;
            }
            catch (Exception ex)
            {
                errors.Add($"Applying edits for Job ID {expectedJobId.Value} failed: {ex.Message}");
                continue;
            }
        }

        return new()
        {
            ProcessedFiles = processedFiles,
            TotalBilledWords = totalBilledWords,
            TotalBilledCharacters = totalBilledCharacters,
            Errors = errors,
        };
    }
    
     private async Task<(string jobId, FileReference transformationFileRef, int totalSegments, int processedSegments)> CreateEditBackgroundJob(
        FileReference file, EditContentInBackgroundRequest input)
    {
        await using var stream = await fileManagementClient.DownloadAsync(file);
        using var reader = new StreamReader(stream);
        var contentString = await reader.ReadToEndAsync();

        var content = Transformation.Parse(contentString, file.Name)
            ?? throw new PluginApplicationException("Something went wrong parsing this bilingual file. Please send a copy of this file to the team for inspection!");

        var sourceLanguage = input.SourceLanguage ?? content.SourceLanguage;
        var targetLanguage = input.TargetLanguage ?? content.TargetLanguage;
        var segmentStatesToEstimate = input.EstimateUnitsWhereAllSegmentStates?
            .Select(s => SegmentStateHelper.ToSegmentState(s) ?? SegmentState.Initial)
            ?? new[] { SegmentState.Initial, SegmentState.Translated };
        var segmentStateQualifiersToExclude = input.ExcludeSegmentStateQualifiers ?? Array.Empty<string>();

        if (sourceLanguage is null)
            throw new PluginMisconfigurationException("The source language is not defined in the bilingual file. Please assign the source language in this action.");

        if (targetLanguage is null)
            throw new PluginMisconfigurationException("The target language is not defined in the bilingual file. Please assign the target language in this action.");

        var xliffBatchBuilder = new XliffBatchBuilder();
        var (xliffContent, idMappings) = xliffBatchBuilder.Build(content, segmentStatesToEstimate, segmentStateQualifiersToExclude, sourceLanguage, targetLanguage);

        var totalSegments = content.GetUnits().Select(u => u.Segments.Count).Sum();
        var processedSegments = idMappings.Count;

        using var xliffStream = new MemoryStream(Encoding.UTF8.GetBytes(xliffContent));

        var batchRequest = new TausRequest(ApiEndpoints.EstimateBatchJob, Method.Post, Creds)
            .AddParameter("source_language", sourceLanguage)
            .AddParameter("target_language", targetLanguage)
            .AddFile("file", () => xliffStream, Path.GetFileNameWithoutExtension(file.Name) + ".xliff", MediaTypes.Xliff);

        if (input.DisableApe != true)
            batchRequest.AddParameter("threshold", input.Threshold.ToString(CultureInfo.InvariantCulture));

        var batchResponse = await Client.ExecuteWithErrorHandling<EstimateBatchJob>(batchRequest);

        if (!string.IsNullOrWhiteSpace(batchResponse.ErrorMessage))
            throw new PluginApplicationException(batchResponse.ErrorMessage);

        // Store metadata in transformation for later retrieval
        content.MetaData.Add(new(TransformationTausMetadata.JobIdType, batchResponse.JobId) 
            { Category = [TransformationTausMetadata.Category] });

        var idMappingsJson = JsonConvert.SerializeObject(idMappings);
        content.MetaData.Add(new(TransformationTausMetadata.SegmentIdMappingsType, idMappingsJson) 
            { Category = [TransformationTausMetadata.Category] });

        // Set quality threshold on units that were processed
        foreach (var unit in content.GetUnits())
        {
            unit.Quality.ScoreThreshold = input.Threshold;
        }

        var transformationFileRef = await fileManagementClient.UploadAsync(
            Xliff2Serializer.Serialize(content).ToStream(),
            "application/xliff+xml",
            content.XliffFileName);

        return (batchResponse.JobId, transformationFileRef, totalSegments, processedSegments);
    }

    private async Task<BackgroundFileResult> ApplyEdits(
        Transformation transformation, string completedJobId, BackgroundDownloadRequest request)
    {
        var overThresholdState = SegmentStateHelper.ToSegmentState(request.OverThresholdState ?? string.Empty) ?? SegmentState.Reviewed;

        var billedWords = 0;
        var billedCharacters = 0;
        var totalSegments = transformation.GetUnits().Select(u => u.Segments.Count).Sum();
        var segmentsProcessed = 0;
        var segmentsFinalized = 0;
        var segmentsUnderThreshold = 0;
        var scoreSum = 0f;
        var scoresCount = 0;

        var fileDownloadRequest = new TausRequest(ApiEndpoints.BatchJobDownload, Method.Get, Creds)
            .AddUrlSegment("job_id", completedJobId)
            .AddOrUpdateHeader("Accept", MediaTypes.Xliff);

        var batchResponse = await Client.ExecuteWithRetry(fileDownloadRequest);
            
        if (!batchResponse.IsSuccessful || string.IsNullOrWhiteSpace(batchResponse.Content))
            throw new PluginApplicationException(!string.IsNullOrWhiteSpace(batchResponse.ErrorMessage)
                ? $"{batchResponse.ErrorMessage} ({batchResponse.StatusCode})."
                : $"Batch job results download failed ({batchResponse.StatusCode}).");

        var idMappingsMetadata = transformation.MetaData.Find(m => 
            m.Category.Contains(TransformationTausMetadata.Category)
            && m.Type == TransformationTausMetadata.SegmentIdMappingsType);

        var idMappings = idMappingsMetadata != null
            ? JsonConvert.DeserializeObject<List<SegmentIdMapping>>(idMappingsMetadata.Value) ?? []
            : [];

        var xliffBatchParser = new XliffBatchParser();
        var results = xliffBatchParser.Parse(batchResponse.Content);

        if (results.Count == 0)
            throw new PluginApplicationException("The batch result file did not contain any processed segments.");

        foreach (var mapping in idMappings)
        {
            if (!results.TryGetValue(mapping.BatchSegmentId, out var result))
                continue;

            var segment = FindSegmentByMapping(transformation, mapping);
            if (segment?.unit == null || segment.Value.segment == null)
                continue;

            var (unit, seg) = segment.Value;

            segmentsProcessed++;
            billedCharacters += result.BilledCharacters;
            
            if (!string.IsNullOrEmpty(result.ApeResult))
            {
                try
                {
                    seg.SetTarget(result.ApeResult);
                }
                catch
                {
                    continue;
                }

                billedWords += result.BilledWords;

                var addLowScoreComment = request.AddLowScoreEditedByTausComment ?? true;
                if (addLowScoreComment
                    && result.ApeScore.HasValue
                    && result.ApeScore.Value < unit.Quality.ScoreThreshold)
                {
                    unit.Notes.Add(new Note("Edited by TAUS") { Reference = seg.Id });
                }
            }

            if (!string.IsNullOrWhiteSpace(result.Remarks))
            {
                unit.Notes.Add(new Note($"Edited by TAUS. {result.Remarks}") { Reference = seg.Id });
            }

            var effectiveScore = result.EffectiveScore;
            if (effectiveScore.HasValue)
            {
                unit.Quality.Score = effectiveScore.Value;
                scoreSum += effectiveScore.Value;
                scoresCount++;
            }

            if (effectiveScore >= unit.Quality.ScoreThreshold)
            {
                seg.State = overThresholdState;
                unit.Quality.Score = effectiveScore;
                unit.Provenance.Review.Tool = "TAUS";
                unit.Notes.Add(new Note($"TAUS QE Score: {effectiveScore:F3} ({unit.Quality.ScoreThreshold:F3})") { Reference = seg.Id });
                segmentsFinalized++;
            }
            else
            {
                segmentsUnderThreshold++;
            }
        }

        // Build result file
        FileReference resultFile;
        if (request.OutputFileHandling == "original")
        {
            var targetContent = transformation.Target();
            resultFile = await fileManagementClient.UploadAsync(
                targetContent.Serialize().ToStream(),
                targetContent.OriginalMediaType,
                targetContent.OriginalName);
        }
        else if (request.OutputFileHandling == "xliff1")
        {
            resultFile = await fileManagementClient.UploadAsync(
                Xliff1Serializer.Serialize(transformation).ToStream(),
                MediaTypes.Xliff,
                transformation.XliffFileName);
        }
        else
        {
            resultFile = await fileManagementClient.UploadAsync(
                transformation.Serialize().ToStream(),
                MediaTypes.Xliff,
                transformation.XliffFileName);
        }

        var averageScore = scoresCount > 0 ? scoreSum / scoresCount : 0f;
        var percentageUnderThreshold = segmentsProcessed > 0 
            ? (float)segmentsUnderThreshold / segmentsProcessed * 100f 
            : 0f;

        return new BackgroundFileResult
        {
            File = resultFile,
            TotalSegments = totalSegments,
            TotalSegmentsProcessed = segmentsProcessed,
            TotalSegmentsFinalized = segmentsFinalized,
            TotalSegmentsUnderThreshhold = segmentsUnderThreshold,
            AverageMetric = averageScore,
            PercentageSegmentsUnderThreshhold = percentageUnderThreshold,
            BilledWords = billedWords,
            BilledCharacters = billedCharacters
        };
    }

    private static (Unit unit, Segment segment)? FindSegmentByMapping(Transformation transformation, SegmentIdMapping mapping)
    {
        // If original ID exists, search by ID
        if (!mapping.IsGenerated && !string.IsNullOrWhiteSpace(mapping.OriginalId))
        {
            foreach (var unit in transformation.GetUnits())
            {
                var segment = unit.Segments.FirstOrDefault(s => s.Id == mapping.OriginalId);
                if (segment != null)
                    return (unit, segment);
            }
        }

        // Fallback: use sequence index
        var currentIndex = 0;
        foreach (var unit in transformation.GetUnits())
        {
            foreach (var segment in unit.Segments)
            {
                if (currentIndex == mapping.SequenceIndex)
                    return (unit, segment);
                currentIndex++;
            }
        }

        return null;
    }
}
