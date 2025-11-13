using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Models.Estimate;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Taus.Connections;

public class ConnectionValidator : IConnectionValidator
{

    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        var client = new TausClient(authProviders);
        var request = new TausRequest(ApiEndpoints.EstimateV2, Method.Post, authProviders)
            .AddJsonBody(new EstimationRequestV2
            {
                Source = new()
                {
                    Value = "Test input",
                    Language = "en"
                },
                Target = new()
                {
                    Value = "Entrada de prueba",
                    Language = "es"
                }
            });

        try
        {
            var response = await client.ExecuteWithErrorHandling<EstimateOutput>(request);
            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}