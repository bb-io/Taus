using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models.Response
{
    public class XliffResponse
    {
        public FileReference File { get; set; }

        public float AverageMetric { get; set; }

      //  public float WeightedMetrics { get; set; }
    }
}
