using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Taus.Models.Request;

public class EstimateInput
{
    [Display("Source text")]
    public string Source { get; set; } = string.Empty;

    [Display("Source language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string SourceLanguage { get; set; } = string.Empty;

    [Display("Translated text")]
    public string Target { get; set; } = string.Empty;

    [Display("Target language")]
    [StaticDataSource(typeof(LanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Label")]
    public string? Label { get; set; }

    [Display("APE threshold")]
    public float? ApeThreshold { get; set; }

    [Display("APE low threshold")]
    public float? ApeLowThreshold { get; set; }

    [Display("Use RAG")]
    public bool? UseRag { get; set; }

    [Display("Threshold", Description = "Threshold score for automatic finalization")]
    public double Threshold { get; set; } = 0.8;
}