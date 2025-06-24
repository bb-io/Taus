using Apps.Taus.DataSourceHandlers;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils.Constants;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Xliff.Utils.Models.Content;
using Blackbird.Xliff.Utils.Serializers.Xliff2;

namespace Apps.Taus.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : TausInvocable(invocationContext)
{
    [Action("Review content", Description = "Estimate translated content returned from other content processing actions")]
    public async Task<ContentReviewResponse> EstimateContent([ActionParameter] EstimateContentRequest input)
    {
        var stream = await fileManagementClient.DownloadAsync(input.File);
        var content = await FileGroup.TryParse(stream);
        if (content == null) throw new PluginApplicationException("Something went wrong parsing this XLIFF file. Please send a copy of this file to the team for inspection!");
        if (content.SourceLanguage == null) throw new PluginMisconfigurationException("The source language is not defined yet. Please assign the source language in this action.");
        if (content.TargetLanguage == null) throw new PluginMisconfigurationException("The target language is not defined yet. Please assign the target language in this action.");

        var srcLanguage = FindTausLanguage(content.SourceLanguage);
        var trgLanguage = FindTausLanguage(content.TargetLanguage);

        var processedSegmentsCount = 0;
        var finalizedSegmentsCount = 0;
        var riskySegmentsCount = 0;
        float totalScore = 0f;

        foreach(var segment in content.IterateSegments().Where(x => !x.Ignorable && !x.IsInitial() && x.State != SegmentState.Final))
        {
            if (segment == null) continue;
            var source = segment.GetSource(TagInclusion.Ignore);
            var target = segment.GetTarget(TagInclusion.Ignore);
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

        var streamResult = Xliff2Serializer.Serialize(content).ToStream();
        var finalFile = await fileManagementClient.UploadAsync(streamResult, input.File.ContentType, input.File.Name);

        return new ContentReviewResponse
        {
            File = finalFile,
            TotalSegmentsFinalized = finalizedSegmentsCount,
            TotalSegmentsProcessed = processedSegmentsCount,
            TotalSegmentsUnderThreshhold = riskySegmentsCount,
            AverageMetric = processedSegmentsCount > 0 ? (totalScore / processedSegmentsCount) : totalScore,
            PercentageSegmentsUnderThreshhold = processedSegmentsCount > 0 ? ((float)riskySegmentsCount / (float)processedSegmentsCount) : riskySegmentsCount,
        };
    }

    private async Task<EstimationResponse> PerformEstimateRequest(string source, string sourceLanguage, string target, string targetLanguage, double threshold = 0.8)
    {
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
        if (!languageExists) throw new PluginMisconfigurationException($"The language {language} is not compatible with the TAUS API.");
        return language!;
    }
}