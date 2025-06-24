using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class EstimationResponse
{
    public Segment Source { get; set; } = new();
    public Segment Target { get; set; } = new();
    [Display("Estimate result")]
    public EstimateResult EstimateResult { get; set; } = new();
    [Display("Ape triggered")]
    public bool ApeTriggered { get; set; }
    [Display("Ape result")]
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
    [Display("APE revisions")]
    public List<ApeRevision> ApeRevisions { get; set; } = new();
    
    [Display("Score")]
    public float Score { get; set; }
    
    [Display("Edit distance")]
    public int EditDistance { get; set; }
    
    [Display("Billed words")]
    public int BilledWords { get; set; }
}

public class ApeRevision
{
    public string Translation { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
