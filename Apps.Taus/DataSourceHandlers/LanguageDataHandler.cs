using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Taus.DataSourceHandlers;

public class LanguageDataHandler : EnumDataHandler
{
    protected override Dictionary<string, string> EnumValues => new()
    {
        { "en", "English" },
        { "fr", "French" },
        { "de", "German" },
        { "it", "Italian" },
        { "es", "Spanish" },
    };
}