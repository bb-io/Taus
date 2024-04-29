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
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new(CredsNames.Url) {DisplayName = "Base URL" },
                new(CredsNames.ApiKey) { DisplayName = "API key", Sensitive = true }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        yield return new(
            AuthenticationCredentialsRequestLocation.None,
            CredsNames.Url,
            values[CredsNames.Url]
        );

        yield return new(
            AuthenticationCredentialsRequestLocation.None,
            CredsNames.ApiKey,
            values[CredsNames.ApiKey]
        );
    }
}