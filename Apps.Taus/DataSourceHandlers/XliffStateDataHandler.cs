using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Taus.DataSourceHandlers
{
    public class XliffStateDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData()
        {
            return new List<DataSourceItem>()
            {
                new DataSourceItem( "final", "final" ),
                new DataSourceItem( "needs-adaptation", "needs-adaptation" ),
                new DataSourceItem( "needs-l10n", "needs-l10n" ),
                new DataSourceItem( "needs-review-adaptation", "needs-review-adaptation" ),
                new DataSourceItem( "needs-review-l10n", "needs-review-l10n" ),
                new DataSourceItem( "needs-review-translation", "needs-review-translation" ),
                new DataSourceItem( "needs-translation", "needs-translation" ),
                new DataSourceItem( "new", "new" ),
                new DataSourceItem( "signed-off", "signed-off" ),
                new DataSourceItem( "translated", "translated"),
            };
        }
    }
}
