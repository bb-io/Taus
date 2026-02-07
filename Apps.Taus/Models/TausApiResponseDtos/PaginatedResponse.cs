using Newtonsoft.Json;

namespace Apps.Taus.Models.TausApiResponseDtos;

public class PaginatedResponse<T>
{
    [JsonProperty("items")]
    public List<T> Items { get; set; } = [];

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("page_size")]
    public int PageSize { get; set; }

    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
}
