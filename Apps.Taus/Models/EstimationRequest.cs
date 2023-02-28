using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models
{
    public class EstimationRequest
    {
        public Segment source { get; set; }
        public List<Segment> targets { get; set; }
    }
}
