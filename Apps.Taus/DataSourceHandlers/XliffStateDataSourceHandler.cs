using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Filters.Enums;

namespace Apps.Taus.DataSourceHandlers;

internal class XliffStateDataSourceHandler
{
    public static IEnumerable<DataSourceItem> GetData() =>
    [
        new(SegmentStateHelper.Serialize(SegmentState.Initial), "Initial or empty"),
        new(SegmentStateHelper.Serialize(SegmentState.Translated), "Translated"),
        new(SegmentStateHelper.Serialize(SegmentState.Reviewed), "Reviewed"),
        new(SegmentStateHelper.Serialize(SegmentState.Final), "Final"),
    ];
}
