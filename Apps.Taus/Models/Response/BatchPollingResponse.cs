using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class BatchPollingResponse
{
    [Display("Completed job IDs")]
    public IEnumerable<string> CompletedJobIds { get; set; } = [];

    [Display("Failed job IDs")]
    public IEnumerable<string> FailedJobIds { get; set; } = [];

    [Display("Expired job IDs")]
    public IEnumerable<string> ExpiredJobIds { get; set; } = [];
}
