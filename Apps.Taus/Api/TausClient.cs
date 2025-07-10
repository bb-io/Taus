using Apps.Taus.Constants;
using Apps.Taus.Models.Response.Error;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace Apps.Taus.Api;

public class TausClient : BlackBirdRestClient
{
    private const int MaxRetries = 5;
    private const int InitialDelayMs = 1000;

    public TausClient(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new RestClientOptions()
    {
        BaseUrl = creds.Get(CredsNames.Url).Value.ToUri()
    })
    {
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
        {
            return new PluginApplicationException($"API response is empty or missing content. Status: {response.StatusCode}. Please verify the request and API availability.");
        }

        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response.Content)!;

        if (errorResponse == null)
        {
            return new PluginApplicationException(response.ErrorException.Message);
        }

        var errors = errorResponse.Errors?.SelectMany(x => x.Values).ToList();

        string errorMessage;

        if (string.IsNullOrEmpty(errorResponse.Message) && (errors == null || !errors.Any()))
        {
            errorMessage = $"Error with status {response.StatusCode}. Response content: {response.Content}";
        }
        else
        {
            errorMessage = $"{errorResponse.Message ?? "No error message provided by the API."};";
        }

        return new PluginApplicationException(errorMessage);
    }

    public override async Task<T> ExecuteWithErrorHandling<T>(RestRequest request)
    {
        string content = (await ExecuteWithErrorHandling(request)).Content;
        T val = JsonConvert.DeserializeObject<T>(content, JsonSettings);
        if (val == null)
        {
            throw new Exception($"Could not parse {content} to {typeof(T)}");
        }

        return val;
    }

    public async Task<RestResponse> ExecuteWithHandling(RestRequest request)
    {
        int delay = InitialDelayMs;
        RestResponse? response = null;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            response = await ExecuteAsync(request);

            if (response.IsSuccessful)
                return response;

            if (attempt < MaxRetries &&
                (response.StatusCode == HttpStatusCode.InternalServerError ||
                 response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                 response.StatusCode == HttpStatusCode.BadRequest ||
                 response.StatusCode == HttpStatusCode.TooManyRequests))
            {
                await Task.Delay(delay);
                delay *= 2;
                continue;
            }
            break;
        }
        throw ConfigureErrorException(response);
    }
}