using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class BackgroundContentResponse
{
    [Display("Processed files")]
    public IEnumerable<BackgroundFileResult> ProcessedFiles { get; set; } = [];

    [Display("Errors")]
    public IEnumerable<string> Errors { get; set; } = [];

    [Display("Total billed words (APE)")]
    public int TotalBilledWords { get; set; }

    [Display("Total billed characters (QE)")]
    public int TotalBilledCharacters { get; set; }
}
