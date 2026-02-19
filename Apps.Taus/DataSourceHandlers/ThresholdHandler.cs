using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Taus.DataSourceHandlers
{
    public class ThresholdHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData()
        {
            return new List<DataSourceItem>()
            {
                new DataSourceItem("1.0", "1.0 | Zero-tolerance mode" ),
                new DataSourceItem("0.9", "0.9 | Enterprise-grade caution" ),
                new DataSourceItem("0.8", "0.8 | Trust, but verify" ),
                new DataSourceItem("0.7", "0.7 | I'm feeling bold today" ),
                new DataSourceItem("0.6", "0.6 | Ship it energy"),
            };
        }
    }
}
