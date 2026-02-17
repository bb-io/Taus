using Apps.Taus.Api;
using Apps.Taus.Constants;
using Apps.Taus.Invocables;
using Apps.Taus.Models.Request;
using Apps.Taus.Models.Response;
using Apps.Taus.Models.TausApiResponseDtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
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
        [PollingEventParameter] OnBatchFinishedRequest input)
    {
        var jobIdsUniqueSet = input.JobIds.ToHashSet();

        if (jobIdsUniqueSet.Count == 0)
            throw new PluginMisconfigurationException("At least one Job ID must be provided.");

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
            .Where(j => jobIdsUniqueSet.Contains(j.JobId) && terminalStatuses.Contains(j.Status))
            .ToList();

        var expectedJobsTerminatedIdsSet = expectedJobsTerminated.Select(j => j.JobId).ToHashSet();

        if (!jobIdsUniqueSet.SetEquals(expectedJobsTerminatedIdsSet))
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
