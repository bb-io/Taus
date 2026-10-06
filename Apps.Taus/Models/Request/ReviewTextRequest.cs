using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Taus.Models.Request
{
    public class ReviewTextRequest : IReviewTextInput
    {
        [Display("Source text")]
        public string SourceText { get; set; }

        [Display("Source language")]
        [StaticDataSource(typeof(LanguageDataHandler))]
        public string SourceLanguage { get; set; }

        [Display("Target text")]
        public string TargetText { get; set; }

        [Display("Target language")]
        [StaticDataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }

        [Display("Metric UID",
            Description = "Metric identifier, such as taus_qe, taus_qe_frca, taus_linguistic_qe, or a custom model UUID. Leave empty for automatic metric selection.")]
        public string? MetricUid { get; set; }

        [Display("Metric version",
            Description = "Version of the selected metric. Leave empty to use its latest version. Requires Metric UID.")]
        public string? MetricVersion { get; set; }
    }
}
