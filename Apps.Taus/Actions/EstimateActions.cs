using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Taus.Actions;

[ActionList]
public class EstimateActions : TausInvocable
{
    public EstimateActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Estimate", Description = "Get estimation data for a segment")]
    public async Task<Metric> Estimate([ActionParameter] EstimateInput estimateInput)
    {
        var request = new TausRequest(ApiEndpoints.Estimate, Method.Post, Creds)
            .AddJsonBody(new EstimationRequest
            {
                Source = new()
                {
                    Value = estimateInput.Source,
                    Language = estimateInput.SourceLanguage,
                    Label = estimateInput.SourceLabel
                },
                Targets = new()
                {
                    new()
                    {
                        Value = estimateInput.Target,
                        Language = estimateInput.TargetLanguage,
                        Label = estimateInput.TargetLabel
                    }
                }
            });

        var response = await Client.ExecuteWithErrorHandling<EstimationResponse>(request);
        return response.Estimates.First().Metrics.First();
    }
}