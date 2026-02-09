using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Response;

public class ContentEditInBackgroundResponse
{
    [Display("Job IDs")]
    public IEnumerable<string> JobIds { get; set; } = [];

    [Display("Transformation files")]
    public IEnumerable<FileReference> TransformationFiles { get; set; } = [];

    [Display("Errors")]
    public IEnumerable<string> JobCreationErrors { get; set; } = [];
}
