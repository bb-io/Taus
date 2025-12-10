using Apps.Taus.Invocables;
using Apps.Taus.Models.Estimate;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;

namespace Apps.Taus.Actions;

[ActionList("Review")]
public class ReviewActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : TausInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.ReviewFile)]
    [Action("Review", Description = "Review a translated content file returned from other content processing actions")]
    public async Task<ContentReviewResponse> EstimateContent([ActionParameter] ReviewContentRequest input)
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
        float totalScore = 0f;
        int billedCharacters = 0;

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
            foreach (var (segment, result) in results)
            {
                billedCharacters += result.EstimateResult.BilledCharacters;
                localBilledCharacters += result.EstimateResult.BilledCharacters;
                var score = result.EstimateResult?.Score;

                if (score == null) continue;
                processedSegmentsCount++;
                totalScore += score.Value;
                unitScore += score.Value;

                if (score >= input.Threshold)
                {
                    segment.State = SegmentState.Final;
                    finalizedSegmentsCount++;
                }
                else
                {
                    riskySegmentsCount++;
                }

            }

            unit.Quality.ProfileReference = "https://api.taus.net/2.0/estimate";
            unit.Quality.ScoreThreshold = input.Threshold;
            unit.Quality.Score = unitScore / results.Count();
            unit.AddUsage("TAUS QE", localBilledCharacters, UsageUnit.Characters);
        }

        Stream streamResult;
        if (input.OutputFileHandling == "original")
        {
            if (Xliff1Serializer.IsXliff1(contentString))
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                streamResult = xliff1String.ToStream();
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

        var finalFile =
            await fileManagementClient.UploadAsync(streamResult, input.File.ContentType, input.File.Name);

        return new ContentReviewResponse
        {
            File = finalFile,
            TotalSegmentsFinalized = finalizedSegmentsCount,
            TotalSegmentsProcessed = processedSegmentsCount,
            TotalSegmentsUnderThreshhold = riskySegmentsCount,
            AverageMetric = processedSegmentsCount > 0 ? (totalScore / processedSegmentsCount) : totalScore,
            BilledCharacters = billedCharacters,
            PercentageSegmentsUnderThreshhold = processedSegmentsCount > 0
                ? ((float)riskySegmentsCount / (float)processedSegmentsCount)
                : riskySegmentsCount,
        };
    }

    [BlueprintActionDefinition(BlueprintAction.ReviewText)]
    [Action("Review text", Description = "Reviews translated text and returns a quality score")]
    public async Task<ReviewTextOutput> EstimateTextContent([ActionParameter] ReviewTextRequest input)
    {
        var response = await Estimate(new EstimateInput
        {
            Source = input.SourceText,
            SourceLanguage = input.SourceLanguage,
            Target = input.TargetText,
            TargetLanguage = input.TargetLanguage,
            ApplyApe = false,
        });

        return new ReviewTextOutput(response);
    }
}