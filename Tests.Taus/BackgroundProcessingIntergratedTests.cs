using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Apps.Taus.Polling;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Polling;
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
        var filesToProcess = new List<FileReference> { new() { Name = "xliff-after-xtm.xlf" } };

        //
        // Create batch processing job
        //
        var createBatchJobRequest = new EditContentInBackgroundRequest
        {
            Files = filesToProcess,
            Threshold = 0.8,
        };

        var createBatchJobResponse = await actions.EditContentInBackground(createBatchJobRequest);

        Console.WriteLine("Created batch job output: " + JsonConvert.SerializeObject(createBatchJobResponse, Formatting.Indented));
        Assert.IsTrue(createBatchJobResponse.JobIds.Any());
        Assert.IsFalse(createBatchJobResponse.JobCreationErrors.Any());

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
        var jobIds = createBatchJobResponse.JobIds;

        do
        {
            await Task.Delay(10 * 1000);
            var jobPollingResponse = await pollingList.OnBatchFinished(jobPollingRequest, jobIds);

            if (jobPollingResponse.FlyBird)
            {
                Console.WriteLine("Polling output: " + JsonConvert.SerializeObject(jobPollingResponse, Formatting.Indented));
                Assert.AreEqual(jobIds.Count(), jobPollingResponse.Result?.CompletedJobIds.Count());
                Assert.AreEqual(0, jobPollingResponse.Result?.FailedJobIds.Count());
                Assert.AreEqual(0, jobPollingResponse.Result?.ExpiredJobIds.Count());
                break;
            }
        } while (true);

        //
        // Download processed file
        //
        var request = new BackgroundDownloadRequest
        {
            JobIds = createBatchJobResponse.JobIds,
            TransformationFiles = createBatchJobResponse.TransformationFiles.Select(FileManager.ReferOutputAsync),
            OutputFileHandling = "xliff1",
        };

        var result = await actions.DownloadContentFromBackground(request);

        Console.WriteLine("Download content output: " + JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.ProcessedFiles.Any());
    }
}
