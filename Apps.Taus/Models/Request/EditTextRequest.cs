using Apps.Taus.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;

namespace Apps.Taus.Models.Request
{
    public class EditTextRequest : IEditTextInput
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

        [Display("APE threshold")]
        [StaticDataSource(typeof(ThresholdHandler))]
        public float? ApeThreshold { get; set; }

        [Display("APE low threshold")]
        [StaticDataSource(typeof(ThresholdHandler))]
        public float? ApeLowThreshold { get; set; }

        [Display("Use RAG")]
        public bool? UseRag { get; set; }
    }
}
