using Apps.Taus.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.Net;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_ValidCredentials_ShouldNotFail()
    {
        var validator = new ConnectionValidator();

        var result = await validator.ValidateConnection(Credentials, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_InvalidCredentials_ShouldFail()
    {
        var validator = new ConnectionValidator();

        var newCredentials = Credentials.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
        var result = await validator.ValidateConnection(newCredentials, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }
}