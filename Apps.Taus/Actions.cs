using Apps.Taus.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Taus
{
    [ActionList]
    public class Actions
    {
        [Action("Estimate", Description = "Get estimation data for a segment")]
        public Metric Estimate(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, [ActionParameter] Parameters parameters)
        {
            var client = new TausClient();
            var request = new TausRequest("/1.0/estimate", Method.Post, authenticationCredentialsProviders);
            request.AddJsonBody(new EstimationRequest
            {
                Source = new Segment { Value = parameters.Source, Language = parameters.SourceLanguage },
                Targets = new List<Segment>() { new Segment { Value = parameters.Target, Language = parameters.TargetLanguage } }
            });

            return client.Post<EstimationResponse>(request).Estimates.First().Metrics.First();
        }
    }
}