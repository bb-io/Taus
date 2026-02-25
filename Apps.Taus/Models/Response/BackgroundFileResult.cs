using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Taus.Models.Response;

/// <summary>
/// Represents the result of processing a single file in the background.
/// Implements IReviewFileOutput for blueprint compatibility.
/// </summary>
public class BackgroundFileResult : IReviewFileOutput
{
    [Display("Reviewed file")]
    public FileReference File { get; set; }

    [Display("Total segments")]
    public int TotalSegments { get; set; }

    [Display("Total segments processed")]
    public int TotalSegmentsProcessed { get; set; }

    [Display("Total segments finalized")]
    public int TotalSegmentsFinalized { get; set; }

    [Display("Total segments under threshold")]
    public int TotalSegmentsUnderThreshhold { get; set; }

    [Display("Average score")]
    public float AverageMetric { get; set; }

    [Display("Percentage segments under threshold")]
    public float PercentageSegmentsUnderThreshhold { get; set; }

    [Display("Billed words (APE)")]
    public int BilledWords { get; set; }

    [Display("Billed characters (QE)")]
    public int BilledCharacters { get; set; }
}
