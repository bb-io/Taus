using Apps.Taus.Invocables;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Taus.DataSourceHandlers
{
    public class ModelDataHandler(InvocationContext invocationContext) : TausInvocable(invocationContext), IAsyncDataSourceItemHandler
    {
        public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
        {
            var metrics = await GetMetrics();           

            var items = metrics.Select(x => new DataSourceItem($"{x.Metric.Uid}-{x.Version}", $"{x.Metric.Name} ({x.Version})")) ?? [];
            if (!string.IsNullOrWhiteSpace(context.SearchString))
                return items.Where(x => x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

            return items;
        }        
    }
}
