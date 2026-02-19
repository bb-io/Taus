using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;

namespace Apps.Taus.Models.Request;

public class EditContentRequest : IEditFileInput
{
    public FileReference File { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(ProcessFileFormatHandler))]
    public string? OutputFileHandling { get; set; }

    [Display("APE threshold")]
    //[StaticDataSource(typeof(ThresholdHandler))]
    public float? ApeThreshold { get; set; }

    [Display("APE low threshold")]
    //[StaticDataSource(typeof(ThresholdHandler))]
    public float? ApeLowThreshold { get; set; }

    [Display("Use RAG")]
    public bool? UseRag { get; set; }

    [Display("Score Threshold", Description = "Threshold score for APE activation vs. automatic finalization")]
    //[StaticDataSource(typeof(ThresholdHandler))]
    public float Threshold { get; set; } = 0.85f;

}
