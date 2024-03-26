﻿using Apps.Taus.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using RestSharp;
using System.Xml.Linq;
using Apps.Taus.Models;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace Apps.Taus.Actions;

    public class XliffActions : TausInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;
        public XliffActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(
           invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Estimate XLIFF", Description = "Get estimation data for a segment")]
        public async Task<XliffResponse> EstimateXliff([ActionParameter] EstimateXliffInput Input)
        {
            var _file = await _fileManagementClient.DownloadAsync(Input.File);

            var transunits = ExtractSegmentsFromXliff(_file);

            var results = new Dictionary<string, float>();

            var file = await _fileManagementClient.DownloadAsync(Input.File);
            string fileContent;
            Encoding encoding;
            using (var inFileStream = new StreamReader(file, true))
            {
                encoding = inFileStream.CurrentEncoding;
                fileContent = inFileStream.ReadToEnd();
            }

            foreach (var transunit in transunits) 
            {
                var request = new TausRequest(ApiEndpoints.Estimate, Method.Post, Creds)
                .AddJsonBody(new EstimationRequest
                {
                    Source = new()
                    {
                        Value = transunit.Source,
                        Language = Input.SourceLang,
                        Label = ""
                    },
                    Targets = new()
                    {
                    new()
                    {
                        Value = transunit.Target,
                        Language = Input.TargetLang,
                        Label = ""
                    }
                    }
                });

            var response = await Client.ExecuteWithErrorHandling<EstimationResponse>(request);
            results.Add(transunit.ID ,response.Estimates.First().Metrics.First().Value);

            fileContent = Regex.Replace(fileContent, @"(<trans-unit id="""+ transunit.ID + @""")", @"${1} extradata="""+ response.Estimates.First().Metrics.First().Value + @"""");

        }

        return new XliffResponse
        {
            AverageMetric = results.Average(x => x.Value),
            File = await _fileManagementClient.UploadAsync(new MemoryStream(encoding.GetBytes(fileContent)), MediaTypeNames.Text.Xml, Input.File.Name)
        };
    }
    public List<TranslationUnit> ExtractSegmentsFromXliff(Stream inputStream)
    {
        var TUs = new List<TranslationUnit>();
        using var reader = new StreamReader(inputStream, Encoding.UTF8);
        var xliffDocument = XDocument.Load(reader);

        XNamespace defaultNs = xliffDocument.Root.GetDefaultNamespace();

        foreach (var transUnit in xliffDocument.Descendants(defaultNs + "trans-unit"))
        {
            TUs.Add(new TranslationUnit
            {
                ID = transUnit.Attribute("id")?.Value,
                Source = transUnit.Element(defaultNs + "source").Value,
                Target = transUnit.Element(defaultNs + "target").Value
            });
        }
        return TUs;
    }

}

    



