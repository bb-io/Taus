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

//[ActionList("Estimate")]
public class EstimateActions(InvocationContext invocationContext) : TausInvocable(invocationContext)
{
    [Action("Estimate", Description = "Get estimation data for a segment")]
    public async Task<EstimationResponse> Estimate([ActionParameter] EstimateInput estimateInput)
    {
        var request = new TausRequest(ApiEndpoints.EstimateV2, Method.Post, Creds)
            .AddJsonBody(new EstimationRequestV2
            {
                Source = new()
                {
                    Value = estimateInput.Source,
                    Language = estimateInput.SourceLanguage
                },
                Target = new()
                {
                    Value = estimateInput.Target,
                    Language = estimateInput.TargetLanguage
                },
                Label = estimateInput.Label,
                ApeConfig = estimateInput.ApplyApe.HasValue && estimateInput.ApplyApe.Value ? new ApeConfig
                {
                    Threshold = estimateInput.ApeThreshold ?? 1,
                    LowThreshold = estimateInput.ApeLowThreshold ?? 0,
                    UseRag = estimateInput.UseRag ?? false
                } : null
            });

        var response = await Client.ExecuteWithErrorHandling<EstimationResponse>(request);
        return response;
    }
}