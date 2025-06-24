using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class EstimationResponse
{
    public Segment Source { get; set; } = new();
    public Segment Target { get; set; } = new();
    public EstimateResult EstimateResult { get; set; } = new();
    public bool ApeTriggered { get; set; }
    public ApeResult ApeResult { get; set; } = new();
    public string Label { get; set; } = string.Empty;
}

public class EstimateResult
{
    public MetricInfo Metric { get; set; } = new();
    
    [Display("Score")]
    public float Score { get; set; }
    
    [Display("Billed Characters")]
    public int BilledCharacters { get; set; }
}

public class MetricInfo
{
    [Display("Metric ID")]
    public string Uid { get; set; } = string.Empty;
    
    public string Version { get; set; } = string.Empty;
}

public class ApeResult
{
    [Display("APE Revisions")]
    public List<ApeRevision> ApeRevisions { get; set; } = new();
    
    [Display("Score")]
    public float Score { get; set; }
    
    [Display("Edit Distance")]
    public int EditDistance { get; set; }
    
    [Display("Billed Words")]
    public int BilledWords { get; set; }
}

public class ApeRevision
{
    public string Translation { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
