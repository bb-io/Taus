using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Request;

public class BackgroundDownloadRequest
{
    [Display("Transformation files", Description = "Expects files from background actions like 'Edit in background'. Job IDs are automatically extracted from file metadata.")]
    public IEnumerable<FileReference> TausTransformationFiles { get; set; } = [];

    [Display("State to set above threshold", Description = "What state to set when segment is estimated to be above quality threshold. By default, 'Reviewed' state is set.")]
    [StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public string? OverThresholdState { get; set; }

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps.")]
    [StaticDataSource(typeof(TausOutputFileFormatDataHandler))]
    public string? OutputFileHandling { get; set; }

    [Display("Add low-score edited by TAUS comment", Description = "When enabled, adds the comment 'Edited by TAUS' for edited segments whose ape-score is below the stored threshold.")]
    public bool? AddLowScoreEditedByTausComment { get; set; } = true;
}
