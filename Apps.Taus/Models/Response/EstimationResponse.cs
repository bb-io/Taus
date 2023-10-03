namespace Apps.Taus.Models.Response;

public class EstimationResponse
{
    public Segment Source { get; set; }
    public List<Estimate> Estimates { get; set; }
}