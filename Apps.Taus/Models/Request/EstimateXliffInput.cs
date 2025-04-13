using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using Blackbird.Applications.Sdk.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Taus.Models.Request
{
    public class EstimateXliffInput
    {
        public FileReference File { get; set; }

        [Display("Source Language")]
        [StaticDataSource(typeof(LanguageDataHandler))]
        public string SourceLang { get; set; }

        [Display("Target Language")]
        [StaticDataSource(typeof(LanguageDataHandler))]
        public string TargetLang { get; set; }

        public IEnumerable<double>? Threshold { get; set; }

        [StaticDataSource(typeof(ConditionDataHandler))]
        public IEnumerable<string>? Condition { get; set; }

        [Display("New Target State")]
        [StaticDataSource(typeof(XliffStateDataHandler))]
        public IEnumerable<string>? State { get;set; }

    }
}
