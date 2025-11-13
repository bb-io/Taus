using Apps.Taus.Models.Estimate;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;

namespace Apps.Taus.Models.Response;
public class EditTextOutput(EstimateOutput response, string original) : IEditTextOutput
{
    public Segment Source { get; set; } = response.Source;
    public Segment Target { get; set; } = response.Target;

    [Display("Score")]
    public float Score { get; set; } = response.ApeResult?.Score ?? response.EstimateResult.Score;

    [Display("Edit distance")]
    public int EditDistance { get; set; } = response.ApeResult?.EditDistance ?? 0;

    [Display("Billed words")]
    public int BilledWords { get; set; } = response.ApeResult?.BilledWords ?? 0;

    [Display("Billed characters")]
    public int BilledCharacters { get; set; } = response.EstimateResult.BilledCharacters;

    [Display("Edited text")]
    public string EditedText { get; set; } = response.ApeResult?.ApeRevisions.LastOrDefault()?.Translation ?? original;

    [Display("Remarks")]
    public string Remarks { get; set; } = response.ApeResult?.ApeRevisions.LastOrDefault()?.Remarks ?? string.Empty;

}