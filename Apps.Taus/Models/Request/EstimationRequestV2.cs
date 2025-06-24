using Apps.Taus.Models;

namespace Apps.Taus.Models.Request;

public class EstimationRequestV2
{
    public Segment Source { get; set; }
    public Segment Target { get; set; }
    public string? Label { get; set; }
    public MetricRequest? Metric { get; set; }
    public ApeConfig? ApeConfig { get; set; }
}

public class MetricRequest
{
    public string Uid { get; set; }
    public string Version { get; set; }
}

public class ApeConfig
{
    public float Threshold { get; set; } = 1;
    public float LowThreshold { get; set; } = 0;
    public bool UseRag { get; set; } = false;
}
