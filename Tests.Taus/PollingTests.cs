using Apps.Taus.Models.Request;
using Apps.Taus.Polling;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class PollingTests : TestBase
{
    [TestMethod]
    public async Task OnBatchFinished_IsSuccess()
    {
        // Arrange
        var pollingList = new BatchPollingList(InvocationContext);
        var request = new PollingEventRequest<BatchMemory>()
        {
            Memory = new()
            {
                LastPollingTime = DateTime.UtcNow.AddMinutes(-210),
                Triggered = false
            }
        };
        var jobIds = new OnBatchFinishedRequest()
        {
            TausBackgroundJobIds =
            [
                "1cbffa11-b71e-428e-be48-aa0549ef5045",
                "75cdf077-68ef-4bea-960f-2e7bdda9d2b8",
            ]
        };

        // Act
        var response = await pollingList.OnBatchFinished(request, jobIds);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        Assert.IsNotNull(response.Result);
        Assert.IsTrue(response.FlyBird);
    }
}
