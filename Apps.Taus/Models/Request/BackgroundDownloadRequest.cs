using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Request;

public class BackgroundDownloadRequest
{
    [Display("Job IDs")]
    public IEnumerable<string> TausBackgroundJobIds { get; set; } = [];

    [Display("Transformation files", Description = "Expects files from background actions like 'Edit in background'.")]
    public IEnumerable<FileReference> TausTransformationFiles { get; set; } = [];

    [Display("State to set above threshold", Description = "What state to set when segment is estimated to be above quality threshold. By default, 'Reviewed' state is set.")]
    [StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public string? OverThresholdState { get; set; }

    [Display("Segment states to estimate", Description = "Only units with at least one segment in the selected states will be included in estimation ('initial' and 'translated' states by default).")]
    [StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public IEnumerable<string>? EstimateUnitsWhereAllSegmentStates { get; set; }

    [Display("Exlude segment state qualifiers", Description = "Segments with the specified qualifiers are excluded from editing. If no qualifiers are provided, all segments are included in the APE job. For XTM, it's recommended to use 'leveraged-tm' and 'leveraged-inherited'.")]
    public IEnumerable<string>? ExcludeSegmentStateQualifiers { get; set; }

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps.")]
    [StaticDataSource(typeof(TausOutputFileFormatDataHandler))]
    public string? OutputFileHandling { get; set; }
}
