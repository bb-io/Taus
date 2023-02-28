using Apps.Taus.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.Text.Json;
using System.Text;

var tausRequest = new EstimationRequest
{
    source = new Segment { value = "Hallo. Dit is een tekst geschreven in het Nederlands. Pas als het vertaald is in het Engels kan jij het ook lezen!", language = "NL" },
    targets = new List<Segment>() { new Segment { value = "Hello, this is a text written in Dutch. Only when it is translated into English can you read it too!", language = "EN" } }
};

var json = JsonSerializer.Serialize(tausRequest);
Console.WriteLine(json);

var httpClient = new HttpClient();
var httpRequest = new HttpRequestMessage()
{
    RequestUri = new Uri("https://api.sandbox.taus.net/1.0/estimate"),
    Method = HttpMethod.Post
};


httpRequest.Headers.Add("api-key", "96f5f219-0227-4292-a4fc-891f4802e199");
httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

var response = httpClient.Send(httpRequest);
var responseString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
Console.WriteLine(responseString);
var result = JsonSerializer.Deserialize<EstimationResponse>(responseString);
Console.WriteLine(result.estimates.First().metrics.First().value);