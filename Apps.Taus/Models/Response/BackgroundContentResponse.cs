using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Response;

public class BackgroundContentResponse
{
    [Display("Processed files")]
    public IEnumerable<FileReference> ProcessedFiles { get; set; } = [];

    [Display("Errors")]
    public IEnumerable<string> Errors { get; set; } = [];
}
