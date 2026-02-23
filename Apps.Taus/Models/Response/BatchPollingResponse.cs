using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class BatchPollingResponse
{
    [Display("Completed job IDs")]
    public IEnumerable<string> TausCompletedJobIds { get; set; } = [];

    [Display("Failed job IDs")]
    public IEnumerable<string> TausFailedJobIds { get; set; } = [];

    [Display("Expired job IDs")]
    public IEnumerable<string> TausExpiredJobIds { get; set; } = [];
}
