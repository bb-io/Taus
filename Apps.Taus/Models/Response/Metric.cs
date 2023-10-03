using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus.Models.Response;

public class Metric
{
    [Display("UID")]
    public string Uid { get; set; }
    
    public float Value { get; set; }
    
    public string Version { get; set; }
}