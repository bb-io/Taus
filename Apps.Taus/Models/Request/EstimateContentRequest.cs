using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Request;
public class EstimateContentRequest
{
    public FileReference File { get; set; }

    [Display("Score threshhold", Description = "All segments above this score will automatically be finalized")]
    public double Threshhold { get; set; }
}
