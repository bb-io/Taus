using Apps.Taus.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Taus
{
    [ActionList]
    public class Actions
    {

        [Action]
        public Metric Estimate(AuthenticationCredentialsProvider authenticationCredentialsProvider, [ActionParameter] Parameters parameters)
        {
            var client = new RestClient("https://api.sandbox.taus.net");
            var request = new RestRequest("/1.0/estimate", Method.Post);
            request.AddHeader("api-key", authenticationCredentialsProvider.Value);
            request.AddJsonBody(new EstimationRequest
            {
                Source = new Segment { Value = parameters.Source, Language = parameters.SourceLanguage },
                Targets = new List<Segment>() { new Segment { Value = parameters.Target, Language = parameters.TargetLanguage } }
            });

            return client.Post<EstimationResponse>(request).Estimates.First().Metrics.First();
        }
    }
}