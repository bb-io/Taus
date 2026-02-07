using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;

namespace Apps.Taus.Models.Request;

public class EditContentInBackgroundRequest
{
    public IEnumerable<FileReference> Files { get; set; } = [];

    [Display("Score Threshold", Description = "Segments scoring below the threshold are automatically post-edited. A new QE score is computed for the post-edited translation. The edited result is returned only if the post-edited translation improves the QE score. If no threshold is provided, APE is disabled for the job.")]
    public double Threshold { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }
}
