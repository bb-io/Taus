using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models.Request;
public class EstimateContentRequest
{
    public FileReference File { get; set; }

    [Display("Score threshhold", Description = "All segments above this score will automatically be finalized")]
    public double Threshhold { get; set; }
}
