namespace Apps.Taus.Models.Mxliff;

/// <summary>
/// Represents the result of processing a single segment by TAUS APE/QE batch API.
/// Parsed from MXLIFF response.
/// </summary>
public sealed class MxliffSegmentResult
{
    /// <summary>
    /// Segment ID from trans-unit
    /// </summary>
    public string SegmentId { get; init; } = string.Empty;

    /// <summary>
    /// Quality estimation score (0.0 - 1.0)
    /// From: &lt;m:trans-prop name="target-quality"&gt;
    /// </summary>
    public float? QualityScore { get; init; }

    /// <summary>
    /// Automatic Post-Editing result text
    /// From: &lt;target&gt; element (if APE improved the translation)
    /// </summary>
    public string? ApeText { get; init; }

    /// <summary>
    /// Original target text (before APE)
    /// </summary>
    public string OriginalTarget { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if segment was processed by TAUS
    /// From: &lt;m:trans-prop name="processed-by"&gt;taus&lt;/m:trans-prop&gt;
    /// </summary>
    public bool ProcessedByTaus { get; init; }

    /// <summary>
    /// Billed characters for QE
    /// </summary>
    public int BilledCharacters { get; init; }

    /// <summary>
    /// Billed words for APE (only when APE was applied)
    /// </summary>
    public int BilledWords { get; init; }

    /// <summary>
    /// Indicates if this segment was locked in the input
    /// </summary>
    public bool WasLocked { get; init; }
}
