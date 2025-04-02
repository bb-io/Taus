using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Taus.DataSourceHandlers
{
    public class ConditionDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData()
        {
            return new List<DataSourceItem>()
            {
                new DataSourceItem(">", "Score is above threshold" ),
                new DataSourceItem(">=", "Score is above or equal threshold" ),
                new DataSourceItem("=", "Score is same as threshold" ),
                new DataSourceItem("<", "Score is below threshold" ),
                new DataSourceItem("<=", "Score is below or equal threshold"),
            };
        }
    }
}
