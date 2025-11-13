namespace Apps.Taus.Models.Estimate;

public class EstimateOutput
{
    public Segment Source { get; set; } = new();
    public Segment Target { get; set; } = new();
    public EstimateResult EstimateResult { get; set; } = new();
    public ApeResult ApeResult { get; set; } = new();
}

public class EstimateResult
{
    public float Score { get; set; }
    public int BilledCharacters { get; set; }
}

public class ApeResult
{
    public List<ApeRevision> ApeRevisions { get; set; } = new();    
    public float Score { get; set; }
    public int EditDistance { get; set; }
    public int BilledWords { get; set; }
}

public class ApeRevision
{
    public string Translation { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
