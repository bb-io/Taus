namespace Apps.Taus.Models.Mxliff;

/// <summary>
/// Represents mapping between original segment IDs and generated IDs for segments without IDs.
/// This allows us to match TAUS batch results back to the original content.
/// </summary>
public sealed class SegmentIdMapping
{
    /// <summary>
    /// Original segment ID (may be empty if generated)
    /// </summary>
    public string OriginalId { get; init; } = string.Empty;

    /// <summary>
    /// ID used in MXLIFF file (generated if original was empty)
    /// </summary>
    public string MxliffId { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if the ID was auto-generated
    /// </summary>
    public bool IsGenerated { get; init; }

    /// <summary>
    /// Zero-based index of the segment in the processing order
    /// </summary>
    public int SequenceIndex { get; init; }
}
