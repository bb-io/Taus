namespace Apps.Taus.Models.XliffBatch;

public sealed class XliffBatchSegmentResult
{
    public string SegmentId { get; init; } = string.Empty;

    public float? Score { get; init; }

    public float? ApeScore { get; init; }

    public float? EffectiveScore => ApeScore ?? Score;

    public string? ApeResult { get; init; }

    public string? Remarks { get; init; }

    public int BilledCharacters { get; init; }

    public int BilledWords { get; init; }
}
