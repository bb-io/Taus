using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Apps.Taus.Polling;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Filters.Enums;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class BackgroundProcessingIntergratedTests : TestBase
{
    [TestMethod]
    public async Task EditContentInBackground_Works()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var filesToProcess = new List<FileReference> { new() { Name = "Sample text.html.xlf" } };

        //
        // Create batch processing job
        //
        var createBatchJobRequest = new EditContentInBackgroundRequest
        {
            Files = filesToProcess,
            Threshold = 0.8f,
        };

        var createBatchJobResponse = await actions.EditContentInBackground(createBatchJobRequest);

        Console.WriteLine("Created batch job output: " + JsonConvert.SerializeObject(createBatchJobResponse, Formatting.Indented));
        Assert.IsTrue(createBatchJobResponse.TausBackgroundJobIds.Any());
        Assert.IsFalse(createBatchJobResponse.TausJobCreationErrors.Any());

        //
        // Wait for batch job to complete
        //
        var pollingList = new BatchPollingList(InvocationContext);
        var jobPollingRequest = new PollingEventRequest<BatchMemory>()
        {
            Memory = new()
            {
                LastPollingTime = DateTime.UtcNow.AddMinutes(-10),
                Triggered = false
            }
        };
        var jobIds = new OnBatchFinishedRequest()
        {
            TausBackgroundJobIds = createBatchJobResponse.TausBackgroundJobIds,
        };

        do
        {
            await Task.Delay(10 * 1000);
            var jobPollingResponse = await pollingList.OnBatchFinished(jobPollingRequest, jobIds);

            if (jobPollingResponse.FlyBird)
            {
                Console.WriteLine("Polling output: " + JsonConvert.SerializeObject(jobPollingResponse, Formatting.Indented));
                Assert.AreEqual(jobIds.TausBackgroundJobIds.Count(), jobPollingResponse.Result?.TausCompletedJobIds.Count());
                Assert.AreEqual(0, jobPollingResponse.Result?.TausFailedJobIds.Count());
                Assert.AreEqual(0, jobPollingResponse.Result?.TausExpiredJobIds.Count());
                break;
            }
        } while (true);

        //
        // Download processed file
        //
        var request = new BackgroundDownloadRequest
        {
            TausBackgroundJobIds = createBatchJobResponse.TausBackgroundJobIds,
            TausTransformationFiles = createBatchJobResponse.TausTransformationFiles.Select(FileManager.ReferOutputAsync),
            OverThresholdState = SegmentStateHelper.Serialize(SegmentState.Final),
            OutputFileHandling = "xliff1",
        };

        var result = await actions.DownloadContentFromBackground(request);

        Console.WriteLine("Download content output: " + JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.ProcessedFiles.Any());
    }
}
