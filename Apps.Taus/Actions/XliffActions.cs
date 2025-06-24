﻿using Apps.Taus.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using RestSharp;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.Taus.Utils;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Xliff.Utils.Extensions;
using System.Globalization;

namespace Apps.Taus.Actions;

[ActionList]
public class XliffActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : TausInvocable(
   invocationContext)
{
    private readonly IFileManagementClient _fileManagementClient = fileManagementClient;

    [Action("Estimate XLIFF", Description = "Gets quality estimation data for all segments in an XLIFF 1.2 file")]
    public async Task<XliffResponse> EstimateXliff([ActionParameter] EstimateXliffInput Input)
    {
        if (string.IsNullOrWhiteSpace(Input.TargetLang))
        {
            throw new PluginMisconfigurationException("Target language must be specified. Please check your input and try again");
        }

        var file = await _fileManagementClient.DownloadAsync(Input.File);
        var memporyStream = new MemoryStream();
        await file.CopyToAsync(memporyStream);
        memporyStream.Position = 0;

        var xliffDocument = memporyStream.ConvertFromXliff();
        var translationUnits = xliffDocument.Files
            .SelectMany(file => file.TranslationUnits)
            .ToList();

        var csvFileStream = await XliffToCsvConverter.ConvertXliffToCsv(xliffDocument);
        var bytes = await csvFileStream.GetByteData();
        var request = new TausRequest(ApiEndpoints.EstimateFileUpload, Method.Post, Creds)
            .AddFile("file", bytes, Input.File.Name, MediaTypeNames.Text.Csv)
            .AddParameter("source_language", Input.SourceLang)
            .AddParameter("target_language", Input.TargetLang);

        var response = await Client.ExecuteWithErrorHandling<EstimateFileUploadResponse>(request);
        foreach (var result in response.Results)
        {
            var translationUnit = translationUnits.FirstOrDefault(x => x.Source.Content == result.Source && x.Target.Content == result.Target);
            if (translationUnit != null)
            {
                var attribute = translationUnit.Attributes.FirstOrDefault(x => x.Key == "extradata");
                if (!string.IsNullOrEmpty(attribute.Key))
                {
                    translationUnit.Attributes.Remove(attribute.Key);
                    translationUnit.Attributes.Add("extradata", result.Score.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    translationUnit.Attributes.Add("extradata", result.Score.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        if (Input.Threshold != null && Input.Condition != null && Input.State != null)
        {
            using var e1 = Input.Threshold.GetEnumerator();
            using var e2 = Input.Condition.GetEnumerator();
            using var e3 = Input.State.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
            {
                var threshold = e1.Current;
                var condition = e2.Current;
                var state = e3.Current;

                var filteredIndexes = new List<int>();
                switch (condition)
                {
                    case ">":
                        filteredIndexes = response.Results.Where(x => x.Score > threshold).Select((x, i) => i).ToList();
                        break;
                    case ">=":
                        filteredIndexes = response.Results.Where(x => x.Score >= threshold).Select((x, i) => i).ToList();
                        break;
                    case "=":
                        filteredIndexes = response.Results.Where(x => x.Score == threshold).Select((x, i) => i).ToList();
                        break;
                    case "<":
                        filteredIndexes = response.Results.Where(x => x.Score < threshold).Select((x, i) => i).ToList();
                        break;
                    case "<=":
                        filteredIndexes = response.Results.Where(x => x.Score <= threshold).Select((x, i) => i).ToList();
                        break;
                }

                filteredIndexes.ForEach(index =>
                {
                    var translationUnit = translationUnits[index];
                    if (translationUnit != null)
                    {
                        var stateAttribute = translationUnit.Target?.Attributes?.FirstOrDefault(x => x.Key == "state");
                        if (stateAttribute != null && !string.IsNullOrEmpty(stateAttribute.Value.Key))
                        {
                            translationUnit.Target?.Attributes?.Remove(stateAttribute.Value.Key);
                            translationUnit.Target?.Attributes?.Add("state", state);
                        }
                        else
                        {
                            translationUnit.Target?.Attributes?.Add("state", state);
                        }
                    }
                });
            }
        }

        var xliffStream = xliffDocument.ConvertToXliff();
        var outputFile = await _fileManagementClient.UploadAsync(xliffStream, MediaTypeNames.Text.Xml, Input.File.Name);
        return new XliffResponse
        {
            AverageMetric = response.Results.Average(x => x.Score),
            File = outputFile
        };
    }
}





