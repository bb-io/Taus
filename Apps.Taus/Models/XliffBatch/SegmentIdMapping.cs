using Newtonsoft.Json;

namespace Apps.Taus.Models.XliffBatch;

public sealed class SegmentIdMapping
{
    public string OriginalId { get; init; } = string.Empty;

    [JsonProperty("MxliffId")]
    public string BatchSegmentId { get; init; } = string.Empty;

    public bool IsGenerated { get; init; }

    public int SequenceIndex { get; init; }
}
