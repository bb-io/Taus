using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models
{
    public class Estimate
    {
        public Segment Segment { get; set; }
        public List<Metric> Metrics { get; set; }
    }
}
