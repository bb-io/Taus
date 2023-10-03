using Apps.Taus.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using RestSharp;

namespace Apps.Taus.Api;

public class TausRequest : RestRequest
{
    public TausRequest(string endpoint, Method method, IEnumerable<AuthenticationCredentialsProvider> creds) : base(
        endpoint, method)
    {
        var apiKey = creds.Get(CredsNames.ApiKey).Value;
        this.AddHeader("api-key", apiKey);
    }
}