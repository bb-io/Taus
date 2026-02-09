using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Estimate;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Apps.Taus.Models.TausApiResponseDtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;
using Blackbird.Filters.Xliff.Xliff2;
using RestSharp;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Segment = Blackbird.Filters.Transformations.Segment;

namespace Apps.Taus.Actions;

[ActionList("Editing")]
public class EditActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : TausInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.EditFile)]
    [Action("Edit", Description = "Edit a translation. This action assumes you have previously translated content in Blackbird through any translation action.")]
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
    [Action("Edit text", Description = "Estimates translated text")]
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

    [Action("Edit in background", Description = "Edits translated text in the background. Use a checkpoint to recover results when they are ready. APE runs asynchronously via OpenAI’s Batch API and may take up to 24 hours, although results are often available significantly earlier.")]
    public async Task<ContentEditInBackgroundResponse> EditContentInBackground([ActionParameter] EditContentInBackgroundRequest input)
    {
        var jobIds = new List<string>();
        var transformationFileRefs = new List<FileReference>();
        var jobCreationErrors = new List<string>();

        foreach (var file in input.Files)
        {
            try
            {
                var (jobId, transformation) = await CreateEditBackgroundJob(file, input);
                jobIds.Add(jobId);
                transformationFileRefs.Add(transformation);
            }
            catch (Exception ex)
            {
                jobCreationErrors.Add($"{file.Name}: {ex.Message}");
            }
        }

        return new()
        {
            JobIds = jobIds,
            TransformationFiles = transformationFileRefs,
            JobCreationErrors = jobCreationErrors,
        };
    }
    
    private async Task<(string, FileReference)> CreateEditBackgroundJob(FileReference file, EditContentInBackgroundRequest input)
    {
        using var stream = await fileManagementClient.DownloadAsync(file);
        using var reader = new StreamReader(stream);
        var contentString = await reader.ReadToEndAsync();

        var content = Transformation.Parse(contentString, file.Name)
            ?? throw new PluginApplicationException("Something went wrong parsing this bilingual file. Please send a copy of this file to the team for inspection!");

        var sourceLanguage = input.SourceLanguage ?? content.SourceLanguage;
        var targetLanguage = input.TargetLanguage ?? content.TargetLanguage;

        if (sourceLanguage is null)
            throw new PluginMisconfigurationException("The source language is not defined in the bilingual file. Please assign the source language in this action.");

        if (targetLanguage is null)
            throw new PluginMisconfigurationException("The target language is not defined in the bilingual file. Please assign the target language in this action.");

        using var tsvStream = new MemoryStream();
        using var tsvWriter = new StreamWriter(tsvStream, new UTF8Encoding(false), leaveOpen: true);

        foreach (var (unit, segment) in GetSegmentsToProcess(content))
        {
            var sourceText = segment.GetSource().Replace("\t", " ").Replace("\n", " ");
            var targetText = segment.GetTarget().Replace("\t", " ").Replace("\n", " ");
            tsvWriter.WriteLine($"{sourceText}\t{targetText}");

            unit.Quality.ScoreThreshold = input.Threshold;
        }

        await tsvWriter.FlushAsync();
        tsvStream.Position = 0;

        var batchRequest = new TausRequest(ApiEndpoints.EstimateBatchJob, Method.Post, Creds)
            .AddParameter("source_language", sourceLanguage)
            .AddParameter("target_language", targetLanguage)
            .AddParameter("ape_threshold", input.Threshold)
            .AddFile("file", () => tsvStream, Path.GetFileNameWithoutExtension(file.Name) + ".tsv", "text/tab-separated-values");

        // TODO Pass error messages back to user when HTTP response is not 200
        var bathResponse = await Client.ExecuteWithErrorHandling<EstimateBatchJob>(batchRequest);

        if (!string.IsNullOrWhiteSpace(bathResponse.ErrorMessage))
            throw new PluginApplicationException(bathResponse.ErrorMessage);

        content.MetaData.Add(new(TransformationTausMetadata.Type, bathResponse.JobId) { Category = [TransformationTausMetadata.Category] });

        var transformationFileRef = await fileManagementClient.UploadAsync(
            Xliff2Serializer.Serialize(content).ToStream(),
            "application/xliff+xml",
            content.XliffFileName);

        return (bathResponse.JobId, transformationFileRef);
    }

    [Action("Download background file", Description = "Download content that was processed in the background. This action should be called after the background process is completed.")]
    public async Task<BackgroundContentResponse> DownloadContentFromBackground([ActionParameter] BackgroundDownloadRequest request)
    {
        var processedFilesRefs = new List<FileReference>();
        var totalBilledWords = 0;
        var totalBilledCharacters = 0;
        var errors = new List<string>();

        foreach (var transformationFileRef in request.TransformationFiles)
        {
            var transformationStream = await fileManagementClient.DownloadAsync(transformationFileRef);
            using var reader = new StreamReader(transformationStream);
            var contentString = await reader.ReadToEndAsync();
            var transformation = Transformation.Parse(contentString, transformationFileRef.Name);

            var expectedJobId = transformation.MetaData.Find(m => 
                m.Category.Contains(TransformationTausMetadata.Category)
                && m.Type == TransformationTausMetadata.Type);

            if (string.IsNullOrWhiteSpace(expectedJobId?.Value))
            {
                errors.Add($"File {transformationFileRef.Name} does not contain the expected metadata to link it to a TAUS batch job.");
                continue;
            }

            try
            {
                var (appliedTransformation, billedWords, billedCharacters) = await ApplyEdits(transformation, expectedJobId.Value, request.OutputFileHandling);
                processedFilesRefs.Add(appliedTransformation);
                totalBilledWords += billedWords;
                totalBilledCharacters += billedCharacters;
            }
            catch (Exception ex)
            {
                errors.Add($"Applying edits for Job ID {expectedJobId.Value} failed: {ex.Message}");
                continue;
            }
        }

        return new()
        {
            ProcessedFiles = processedFilesRefs,
            TotalBilledWords = totalBilledWords,
            TotalBilledCharacters = totalBilledCharacters,
            Errors = errors,
        };
    }

    private async Task<(FileReference, int billedWords, int billedCharacters)> ApplyEdits(
        Transformation transformation, string completedJobId, string? outputFileHandling)
    {
        var billedWords = 0;
        var billedCharacters = 0;

        var fileDownloadRequest = new TausRequest(ApiEndpoints.BatchJobDownload, Method.Get, Creds)
            .AddUrlSegment("job_id", completedJobId)
            .AddOrUpdateHeader("Accept", "text/tab-separated-values");

        var batchResponse = await Client.ExecuteAsync(fileDownloadRequest);
            
        if (!batchResponse.IsSuccessful || batchResponse.Content is null)
            throw new PluginApplicationException(!string.IsNullOrWhiteSpace(batchResponse.ErrorMessage) ? batchResponse.ErrorMessage : "Download failed.");

        var segments = batchResponse.Content
            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
            .Skip(1) // Skip Header
            .Select(line => line.Split('\t'))
            .Select(SegmentsFromBatchApe.FromArray)
            .ToList() ?? [];

        if (segments.Count == 0)
            throw new PluginApplicationException("The batch result file did not contain any segments.");

        foreach (var ((originalUnit, originalSegment), processedSegment) in GetSegmentsToProcess(transformation).Zip(segments))
        {
            originalUnit.Quality.Score = processedSegment.ApeScore ?? processedSegment.Score;
            originalUnit.Provenance.Review.Tool = "TAUS";

            if (originalUnit.Quality.Score < originalUnit.Quality.ScoreThreshold)
                continue;

            originalSegment.State = SegmentState.Reviewed;
            originalUnit.Notes.Add(new Note("Scored above threshold by TAUS") { Reference = originalSegment.Id });

            // The APE result is returned only if the post-edited translation improves the QE score
            if (!string.IsNullOrEmpty(processedSegment.ApeResult))
            {
                originalSegment.SetTarget(processedSegment.ApeResult);
                originalUnit.Notes.Add(new Note(processedSegment.Remarks ?? "Edited by APE") { Reference = originalSegment.Id });
                billedWords += processedSegment.BilledWords ?? 0;
            }
            else
                billedCharacters += processedSegment.BilledCharacters ?? 0;
        }

        FileReference resultFile;
        if (outputFileHandling == "original")
        {
            var targetContent = transformation.Target();
            resultFile = await fileManagementClient.UploadAsync(
                targetContent.Serialize().ToStream(),
                targetContent.OriginalMediaType,
                targetContent.OriginalName);
        }
        else if (outputFileHandling == "xliff1")
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

        return (resultFile, billedWords, billedCharacters);
    }

    private static IEnumerable<(Unit, Segment)> GetSegmentsToProcess(Transformation transformation)
    {
        foreach (var unit in transformation.GetUnits())
        {
            if (unit.IsInitial || unit.State == SegmentState.Final)
                continue;

            foreach (var segment in unit.Segments)
            {
                if (segment.IsIgnorbale || segment.State == SegmentState.Final)
                    continue;

                yield return (unit, segment);
            }
        }
    }
}
