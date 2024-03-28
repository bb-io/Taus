using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using Blackbird.Applications.Sdk.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Taus.Models.Request
{
    public class EstimateXliffInput
    {
        public FileReference File { get; set; }

        [Display("Source Language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string SourceLang { get; set; }

        [Display("Target Language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLang { get; set; }

        public float? Threshold { get; set; }

        [DataSource(typeof(ConditionDataHandler))]
        public string? Condition { get; set; }

        [Display("New Target State")]
        [DataSource(typeof(XliffStateDataHandler))]
        public string? State { get;set; }

    }
}
