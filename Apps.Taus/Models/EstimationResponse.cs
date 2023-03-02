using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models
{
    public class EstimationResponse
    {
        public Segment Source { get; set; }
        public List<Estimate> Estimates { get; set; }
    }
}
