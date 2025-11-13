using Apps.Taus.Models.Estimate;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Taus.Models.Response;
public class ReviewTextOutput(EstimateOutput response) : IReviewTextOutput
{
    public Segment Source { get; set; } = response.Source;
    public Segment Target { get; set; } = response.Target;

    [Display("Score")]
    public float Score { get; set; } = response.EstimateResult.Score;

    [Display("Billed characters")]
    public int BilledCharacters { get; set; } = response.EstimateResult.BilledCharacters;

}