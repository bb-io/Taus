namespace Apps.Taus.Models.Response.Error;

public class ErrorResponse
{
    public string? Message { get; set; }
    public IEnumerable<Dictionary<string, string>>? Errors { get; set; }
}