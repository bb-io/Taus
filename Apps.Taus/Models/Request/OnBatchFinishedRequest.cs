using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Request;

public class OnBatchFinishedRequest
{
    [Display("Job IDs")]
    public IEnumerable<string> JobIds { get; set; } = [];
}
