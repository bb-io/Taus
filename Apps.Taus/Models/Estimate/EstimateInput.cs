namespace Apps.Taus.Models.Estimate;

public class EstimateInput
{
    public string Source { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool? ApplyApe { get; set; }
    public float? ApeThreshold { get; set; }
    public float? ApeLowThreshold { get; set; }
    public bool? UseRag { get; set; }
}