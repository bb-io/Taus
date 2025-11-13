using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;

namespace Apps.Taus.Models.Response;
public class ContentEditResponse : IEditFileOutput
{
    [Display("Edited file")]
    public FileReference File { get; set; }

    [Display("Billed characters")]
    public int BilledCharacters { get; set; }

    [Display("Billed words")]
    public int BilledWords { get; set; }

    [Display("Total segments reviewed")]
    public int TotalSegmentsReviewed { get; set; }

    [Display("Total segments updated")]
    public int TotalSegmentsUpdated { get; set; }

    [Display("Total segments finalized")]
    public int TotalSegmentsFinalized { get; set; }

    [Display("Total segments still under threshold")]
    public int TotalSegmentsUnderThreshhold { get; set; }
}
