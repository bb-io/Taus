using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using Blackbird.Applications.Sdk.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Models.Request
{
    public class EstimateXliffInput
    {
        public FileReference File { get; set; }

        [Display("Source Language")]
        public string SourceLang { get; set; }

        [Display("Target Language")]
        public string TargetLang { get; set; }

    }
}
