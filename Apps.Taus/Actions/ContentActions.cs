using Apps.Taus.DataSourceHandlers;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Apps.Taus.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;

namespace Apps.Taus.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : TausInvocable(invocationContext)
{
    [Action("Review content",
        Description = "Estimate translated content returned from other content processing actions")]
    public async Task<ContentReviewResponse> EstimateContent([ActionParameter] EstimateContentRequest input)
    {
        return await ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
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

            var srcLanguage = FindTausLanguage(content.SourceLanguage);
            var trgLanguage = FindTausLanguage(content.TargetLanguage);

            var processedSegmentsCount = 0;
            var finalizedSegmentsCount = 0;
            var riskySegmentsCount = 0;
            float totalScore = 0f;

            var segments = content.GetUnits().SelectMany(x => x.Segments).ToList();
            var segmentsToProcess = segments.Where(x => !x.IsIgnorbale && x.State != SegmentState.Final);
            foreach (var segment in segmentsToProcess)
            {
                if (segment == null) continue;
                var source = segment.GetSource();
                var target = segment.GetTarget();
                if (target == null) continue;
                var estimate = await PerformEstimateRequest(source, srcLanguage, target, trgLanguage, input.Threshhold);

                var result = estimate.EstimateResult?.Score;
                if (result == null)
                {
                    continue;
                }

                var score = result.Value;
                processedSegmentsCount++;
                totalScore += score;

                if (score >= input.Threshhold)
                {
                    segment.State = SegmentState.Final;
                    finalizedSegmentsCount++;
                }
                else
                {
                    riskySegmentsCount++;
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
                PercentageSegmentsUnderThreshhold = processedSegmentsCount > 0
                    ? ((float)riskySegmentsCount / (float)processedSegmentsCount)
                    : riskySegmentsCount,
            };
        });
    }

    private async Task<EstimationResponse> PerformEstimateRequest(string source, string sourceLanguage, string target,
        string targetLanguage, double threshold = 0.8)
    {
        await Task.Delay(800);

        var estimateActions = new EstimateActions(InvocationContext);
        return await estimateActions.Estimate(new EstimateInput
        {
            Source = source,
            SourceLanguage = sourceLanguage,
            Target = target,
            TargetLanguage = targetLanguage,
            Threshold = threshold
        });
    }

    private static string FindTausLanguage(string language)
    {
        language = language?.Split('-')?.FirstOrDefault()?.ToLower();
        var handler = new LanguageDataHandler();
        var languageExists = handler.GetData().FirstOrDefault(x => x.Value == language) != null;
        if (!languageExists)
            throw new PluginMisconfigurationException($"The language {language} is not compatible with the TAUS API.");
        return language!;
    }
}