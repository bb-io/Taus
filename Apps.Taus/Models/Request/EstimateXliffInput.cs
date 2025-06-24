using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common;
using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Taus.Models.Request;

public class EstimateXliffInput
{
    public FileReference File { get; set; } = new();

    [Display("Source Language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string SourceLang { get; set; } = string.Empty;

    [Display("Target Language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string TargetLang { get; set; } = string.Empty;

    public IEnumerable<double>? Threshold { get; set; }

    [StaticDataSource(typeof(ConditionDataHandler))]
    public IEnumerable<string>? Condition { get; set; }

    [Display("New Target State")]
    [StaticDataSource(typeof(XliffStateDataHandler))]
    public IEnumerable<string>? State { get;set; }

}
