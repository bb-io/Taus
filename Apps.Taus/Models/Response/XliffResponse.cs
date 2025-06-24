using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Response;

public class XliffResponse
{
    public FileReference File { get; set; }

    [Display("Average score")]
    public float AverageMetric { get; set; }

    [Display("Estimated units")]
    public int EstimatedUnits { get; set; }
}
