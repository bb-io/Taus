using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models
{
    public class EstimationRequest
    {
        public Segment Source { get; set; }
        public List<Segment> Targets { get; set; }
    }
}
