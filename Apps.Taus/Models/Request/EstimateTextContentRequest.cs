using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Taus.Models.Request
{
    public class EstimateTextContentRequest : IReviewTextInput
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

        [Display("Label")]
        public string? Label { get; set; }

        [Display("Apply APE")]
        public bool? ApplyApe { get; set; }

        [Display("APE threshold")]
        public float? ApeThreshold { get; set; }

        [Display("APE low threshold")]
        public float? ApeLowThreshold { get; set; }

        [Display("Use RAG")]
        public bool? UseRag { get; set; }

        [Display("Threshold", Description = "Threshold score for automatic finalization")]
        public double Threshold { get; set; } = 0.8;
    }
}
