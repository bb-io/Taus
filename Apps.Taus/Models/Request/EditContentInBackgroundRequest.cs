using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Taus.Models.Request;

public class EditContentInBackgroundRequest
{
    public IEnumerable<FileReference> Files { get; set; } = [];

    [Display("Score threshold", Description = "Segments scoring below the threshold are automatically post-edited. A new QE score is computed for the post-edited translation. The edited result is returned only if the post-edited translation improves the QE score.")]
    [StaticDataSource(typeof(ThresholdHandler))]
    public float Threshold { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Segment states to estimate", Description = "Only units with at least one segment in the selected states will be included in estimation ('initial' and 'translated' states by default).")]
    [StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public IEnumerable<string>? EstimateUnitsWhereAllSegmentStates { get; set; }

    [Display("Exclude segment state qualifiers", Description = "Segments with the specified qualifiers are excluded from editing. If no qualifiers are provided, all segments are included in the APE job. For XTM, it's recommended to use 'leveraged-tm' and 'leveraged-inherited'.")]
    public IEnumerable<string>? ExcludeSegmentStateQualifiers { get; set; }

    [Display("Disable automated post-editing (APE)", Description = "Estimate segments only. APE is enabled by default.")]
    public bool? DisableApe { get; set; }

    [Display("Use RAG with APE", Description = "When enabled, Retrieval-Augmented Generation (RAG) is used to enhance the APE output by providing relevant context from the translation memory. RAG is disabled by default.")]
    public bool? UseRagWithApe { get; set; }

    [Display("APE Resource group ID")]
    public string? ApeResourceGroupId { get; set; }
}
