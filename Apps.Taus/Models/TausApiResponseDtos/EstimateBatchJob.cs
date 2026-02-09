using Newtonsoft.Json;

namespace Apps.Taus.Models.TausApiResponseDtos;

public class EstimateBatchJob
{
    [JsonProperty("id")]
    public string JobId { get; set; } = string.Empty;

    [JsonProperty("subscriber_uid")]
    public string? SubscriberUid { get; set; }

    [JsonProperty("metric")]
    public MetricDto? Metric { get; set; }

    [JsonProperty("source_language")]
    public string? SourceLanguage { get; set; }

    [JsonProperty("target_language")]
    public string? TargetLanguage { get; set; }

    [JsonProperty("ape_threshold")]
    public double? ApeThreshold { get; set; }

    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class MetricDto
{
    [JsonProperty("uid")]
    public string? Uid { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
}

