using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Response;
using Apps.Taus.Models.TausApiResponseDtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.Taus.Polling;

[PollingEventList]
public class BatchPollingList(InvocationContext invocationContext) : TausInvocable(invocationContext)
{
    [PollingEvent("On background job finished", "Triggered when a job reaches a terminal state (completed/failed/cancelled).")]
    public async Task<PollingEventResponse<BatchMemory, BatchPollingResponse>> OnBatchFinished(
        PollingEventRequest<BatchMemory> request,
        [PollingEventParameter, Display("Job IDs")] IEnumerable<string> jobIds)
    {
        var terminalStatuses = new[] { "COMPLETED", "FAILED", "EXPIRED" };
        var lastPollingTime = DateTime.UtcNow;
        var noFlightResponse = new PollingEventResponse<BatchMemory, BatchPollingResponse>()
        {
            FlyBird = false,
            Memory = new()
            {
                LastPollingTime = lastPollingTime,
                Triggered = false
            }
        };

        if (request.Memory is null)
            return noFlightResponse;

        var listJobsRequest = new TausRequest(ApiEndpoints.ListBatchJobs, Method.Get, Creds);
        var listJobsResponse = await Client.Paginate<EstimateBatchJob>(listJobsRequest);

        var expectedJobsTerminated = listJobsResponse
            .Where(j => jobIds.Contains(j.JobId) && terminalStatuses.Contains(j.Status));

        if (expectedJobsTerminated.Count() != jobIds.Count())
            return noFlightResponse;

        return new()
        {
            FlyBird = true,
            Result = new()
            {
                CompletedJobIds = expectedJobsTerminated.Where(j => j.Status == "COMPLETED").Select(j => j.JobId),
                FailedJobIds = expectedJobsTerminated.Where(j => j.Status == "FAILED").Select(j => j.JobId),
                ExpiredJobIds = expectedJobsTerminated.Where(j => j.Status == "EXPIRED").Select(j => j.JobId),
            },
            Memory = new()
            {
                LastPollingTime = lastPollingTime,
                Triggered = true
            }
        };
    }
}
