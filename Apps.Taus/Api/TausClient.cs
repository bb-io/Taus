using Apps.Taus.Constants;
using Apps.Taus.Models.Response.Error;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Taus.Api;

public class TausClient : BlackBirdRestClient
{
    public TausClient() : base(new RestClientOptions()
    {
        BaseUrl = Urls.Api.ToUri()
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