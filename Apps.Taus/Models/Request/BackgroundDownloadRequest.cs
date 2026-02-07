using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;

namespace Apps.Taus.Models.Request;

public class BackgroundDownloadRequest
{
    [Display("Job IDs")]
    public IEnumerable<string> JobIds { get; set; } = [];

    [Display("Transformation files", Description = "Expects files from background actions like 'Edit in background'.")]
    public IEnumerable<FileReference> TransformationFiles { get; set; } = [];

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps.")]
    [StaticDataSource(typeof(ProcessFileFormatHandler))]
    public string? OutputFileHandling { get; set; }
}
