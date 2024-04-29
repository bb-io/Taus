using Apps.Taus.Constants;
using Apps.Taus.Models.Response.Error;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Taus.Api;

public class TausClient : BlackBirdRestClient
{
    public TausClient(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new RestClientOptions()
    {
        BaseUrl = creds.Get(CredsNames.Url).Value.ToUri()
    })
    {
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response.Content)!;

        var errors = errorResponse.Errors?.SelectMany(x => x.Values);
        var errorMessage = errorResponse.Message is null && errors is null
            ? response.StatusDescription
            : $"{errorResponse.Message};{string.Join("; ", errors)}";

        return new(errorMessage);
    }
}