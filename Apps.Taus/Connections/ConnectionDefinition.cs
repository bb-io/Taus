using Apps.Taus.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Taus.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = "Developer API key",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new(CredsNames.Url) 
                {
                    DisplayName = "Environment", 
                    Description = "Production or sandbox",
                    DataItems = [new ("https://api.taus.net", "Production"), new ("https://api.sandbox.taus.net", "Sandbox")]
                },
                new(CredsNames.ApiKey) 
                { 
                    DisplayName = "API key", 
                    Sensitive = true 
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        yield return new(
            CredsNames.Url,
            values[CredsNames.Url]
        );

        yield return new(
            CredsNames.ApiKey,
            values[CredsNames.ApiKey]
        );
    }
}