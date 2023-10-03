using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Taus.Connections;

public class ConnectionValidator : IConnectionValidator
{
    private TausClient Client => new();

    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        var request = new TausRequest(ApiEndpoints.Estimate, Method.Post, authProviders)
            .AddJsonBody(new EstimationRequest
            {
                Source = new()
                {
                    Value = "Test input",
                    Language = "en",
                },
                Targets = new()
                {
                    new()
                    {
                        Value = "Test input",
                        Language = "de",
                    }
                }
            });

        try
        {
            await Client.ExecuteWithErrorHandling<EstimationResponse>(request);

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