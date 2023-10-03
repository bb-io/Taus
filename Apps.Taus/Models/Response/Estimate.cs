namespace Apps.Taus.Models.Response;

public class Estimate
{
    public Segment Segment { get; set; }
    public List<Metric> Metrics { get; set; }
}