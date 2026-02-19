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
                LastPollingTime = DateTime.UtcNow.AddMinutes(-10),
                Triggered = false
            }
        };
        var jobIds = new OnBatchFinishedRequest()
        {
            JobIds =
            [
                "0394f618-e41c-41c1-9d37-417c98cd6f1c",
                "5f346c19-3da0-4bc8-9680-1dff71827908",
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
