using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Taus.Models.Request;

public class EstimateContentRequest : IReviewFileInput
{
    public FileReference File { get; set; }

    [DefinitionIgnore]
    public string? TargetLanguage { get; set; }

    [Display("Score threshold", Description = "All segments above this score will automatically be finalized")]
    public double Threshhold { get; set; }
    
    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(ProcessFileFormatHandler))]
    public string? OutputFileHandling { get; set; }
}
