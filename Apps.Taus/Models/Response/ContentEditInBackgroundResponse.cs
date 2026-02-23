using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Response;

public class ContentEditInBackgroundResponse
{
    [Display("Job IDs")]
    public IEnumerable<string> TausBackgroundJobIds { get; set; } = [];

    [Display("Transformation files")]
    public IEnumerable<FileReference> TausTransformationFiles { get; set; } = [];

    [Display("Errors")]
    public IEnumerable<string> TausJobCreationErrors { get; set; } = [];

    [Display("Total segments")]
    public int TotalSegments { get; set; }

    [Display("Segments sent for editing")]
    public int ProcessedSegments { get; set; }
}
