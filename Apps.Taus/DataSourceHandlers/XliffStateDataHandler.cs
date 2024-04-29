using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.DataSourceHandlers
{
    public class XliffStateDataHandler : IStaticDataSourceHandler
    {
        protected Dictionary<string, string> EnumValues => new()
        {
            { "final", "final" },
            { "needs-adaptation", "needs-adaptation" },
            { "needs-l10n", "needs-l10n" },
            { "needs-review-adaptation", "needs-review-adaptation" },
            { "needs-review-l10n", "needs-review-l10n" },
            { "needs-review-translation", "needs-review-translation" },
            { "needs-translation", "needs-translation" },
            { "new", "new" },
            { "signed-off", "signed-off" },
            { "translated", "translated"}
        };

        public Dictionary<string, string> GetData()
        {
            return EnumValues;
        }
    }
}
