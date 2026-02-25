using Blackbird.Filters.Constants;

namespace Apps.Taus.Constants;

public static class TransformationTausMetadata
{
    public const string JobIdType = "TausEstimateBatchJobID";
    public const string SegmentIdMappingsType = "TausSegmentIdMappings";
    public const string Category = Meta.Categories.Blackbird;
    
    // Legacy support
    [Obsolete("Use JobIdType instead")]
    public const string Type = JobIdType;
}
