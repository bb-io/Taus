using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.DataSourceHandlers;
using Apps.Taus.Models.Estimate;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Taus.Invocables;

public class TausInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected TausClient Client { get; }

    public TausInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(Creds);
    }

    protected async Task<EstimateOutput> Estimate([ActionParameter] EstimateInput estimateInput)
    {
        var request = new TausRequest(ApiEndpoints.EstimateV2, Method.Post, Creds)
            .AddJsonBody(new EstimationRequestV2
            {
                Source = new()
                {
                    Value = estimateInput.Source,
                    Language = FindTausLanguage(estimateInput.SourceLanguage)
                },
                Target = new()
                {
                    Value = estimateInput.Target,
                    Language = FindTausLanguage(estimateInput.TargetLanguage)
                },
                Label = estimateInput.Label,
                ApeConfig = estimateInput.ApplyApe.HasValue && estimateInput.ApplyApe.Value ? new ApeConfig
                {
                    Threshold = estimateInput.ApeThreshold ?? 1,
                    LowThreshold = estimateInput.ApeLowThreshold ?? 0,
                    UseRag = estimateInput.UseRag ?? false
                } : null
            });

        var response = await Client.ExecuteWithErrorHandling<EstimateOutput>(request);
        return response;
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