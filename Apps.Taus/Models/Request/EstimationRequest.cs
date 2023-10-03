namespace Apps.Taus.Models.Request;

public class EstimationRequest
{
    public Segment Source { get; set; }
    public List<Segment> Targets { get; set; }
}