using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models.Response;
public class ContentReviewResponse
{
    [Display("Reviewed file")]
    public FileReference File { get; set; }

    [Display("Total segments processed")]
    public int TotalSegmentsProcessed { get; set; }

    [Display("Total segments finalized")]
    public int TotalSegmentsFinalized { get; set; }

    [Display("Total segments under threshold")]
    public int TotalSegmentsUnderThreshhold { get; set; }

    [Display("Average score")]
    public float AverageMetric { get; set; }

    [Display("Percentage segments under threshold")]
    public float PercentageSegmentsUnderThreshhold { get; set; }
}
