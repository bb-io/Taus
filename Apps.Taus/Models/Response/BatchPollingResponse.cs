using System.ComponentModel.DataAnnotations;

namespace Apps.Taus.Models.Response;

public class BatchPollingResponse
{
    [Display(Name = "Completed job IDs")]
    public IEnumerable<string> CompletedJobIds { get; set; } = [];

    [Display(Name = "Failed job IDs")]
    public IEnumerable<string> FailedJobIds { get; set; } = [];

    [Display(Name = "Expired job IDs")]
    public IEnumerable<string> ExpiredJobIds { get; set; } = [];
}
