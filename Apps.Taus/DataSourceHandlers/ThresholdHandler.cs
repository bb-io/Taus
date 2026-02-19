using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Taus.DataSourceHandlers
{
    public class ThresholdHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData()
        {
            return new DataSourceItem[]
            {
                new DataSourceItem("1.0", "1.0 | Zero-tolerance mode" ),
                new DataSourceItem("0.95", "0.95 | Enterprise-grade caution" ),
                new DataSourceItem("0.85", "0.85 | Trust, but verify" ),
                new DataSourceItem("0.7", "0.70 | I'm feeling bold today" ),
                new DataSourceItem("0.6", "0.60 | Ship it energy")
            };
        }
    }
}
