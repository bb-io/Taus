using Apps.Taus.Models;
using Apps.Taus.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.Text;
using System.Text.Json;

namespace Apps.Taus
{
    [ActionList]
    public class Actions
    {

        [Action]
        public Metric Estimate(AuthenticationCredentialsProvider authenticationCredentialsProvider, [ActionParameter] BlackbirdRequest estimationRequest)
        {
            var tausRequest = new EstimationRequest
            {
                source = new Segment { value = estimationRequest.Source, language = estimationRequest.SourceLanguage },
                targets = new List<Segment>() { new Segment { value = estimationRequest.Target, language = estimationRequest.TargetLanguage } }
            };

            var json = JsonSerializer.Serialize(tausRequest);

            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.sandbox.taus.net/1.0/estimate"),
                Method = HttpMethod.Post
            };            

            httpRequest.Headers.Add("api-key", authenticationCredentialsProvider.Value);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = httpClient.Send(httpRequest);
            var responseString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            var result = JsonSerializer.Deserialize<EstimationResponse>(responseString);

            return result.estimates.First().metrics.First();
        }
    }
}