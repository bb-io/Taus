using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class EstimateFileUploadResponse
{
    [Display("ID")]
    public string Uid { get; set; } = string.Empty;
    
    [Display("Source language")]
    public string SourceLanguage { get; set; } = string.Empty;
    
    [Display("Target language")]
    public string TargetLanguage { get; set; } = string.Empty;
    
    [Display("Billed characters")]
    public int BilledCharacters { get; set; }
    
    public Metric Metric { get; set; } = new();
    
    public List<TranslationResult> Results { get; set; } = new();
}

public class TranslationResult
{
    public string Source { get; set; } = string.Empty;
    
    public string Target { get; set; } = string.Empty;
    
    public float Score { get; set; }
}
