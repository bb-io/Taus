using Apps.Taus.Constants;
using Apps.Taus.Models.Response.Error;
using Apps.Taus.Models.TausApiResponseDtos;
using Apps.Taus.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestSharp;
using System.Net;

namespace Apps.Taus.Api;

public class TausClient : BlackBirdRestClient
{
    private static readonly ResiliencePipeline<RestResponse> TooManyRequestsPipeline =
       TausPollyPolicies.GetTooManyRequestsRetryPolicy(retryCount: 6);

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
        var response = await TooManyRequestsPipeline.ExecuteAsync(
            ct => new ValueTask<RestResponse>(base.ExecuteAsync(request, ct)),
            CancellationToken.None);

        if (!response.IsSuccessStatusCode)
        {
            throw ConfigureErrorException(response);
        }

        T val = JsonConvert.DeserializeObject<T>(response.Content, JsonSettings);
        if (val == null)
        {
            throw new Exception($"Could not parse {response.Content} to {typeof(T)}");
        }

        return val;
    }

    public async Task<List<T>> Paginate<T>(RestRequest request)
    {
        var allItems = new List<T>();
        int currentPage = 1;
        int totalPages;

        do
        {
            request.AddOrUpdateParameter("page", currentPage);
            var response = await ExecuteWithErrorHandling<PaginatedResponse<T>>(request);

            if (response is null)
                break;

            allItems.AddRange(response.Items);
            totalPages = response.TotalPages;

            currentPage++;
        } while (currentPage <= totalPages);

        return allItems;
    }
}