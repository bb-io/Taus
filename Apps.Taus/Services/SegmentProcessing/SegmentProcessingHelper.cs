using Blackbird.Filters.Enums;
using Blackbird.Filters.Transformations;

namespace Apps.Taus.Services.SegmentProcessing;

public static class SegmentProcessingHelper
{
    public static bool ShouldProcessSegment(
        Segment segment,
        IEnumerable<SegmentState>? statesToInclude = null,
        IEnumerable<string>? qualifiersToExclude = null)
    {
        if (segment.IsIgnorbale)
            return false;

        if (statesToInclude != null)
        {
            var segmentState = segment.State ?? SegmentState.Initial;
            var allowedStates = statesToInclude as IReadOnlyCollection<SegmentState> ?? statesToInclude.ToArray();
            if (!allowedStates.Contains(segmentState))
                return false;
        }

        var excludedQualifiers = qualifiersToExclude as IReadOnlyCollection<string> ?? qualifiersToExclude?.ToArray() ?? [];
        if (excludedQualifiers.Count > 0)
        {
            var stateQualifier = segment.TargetAttributes.FirstOrDefault(a => a.Name == "state-qualifier");
            if (stateQualifier is not null && excludedQualifiers.Contains(stateQualifier.Value))
                return false;
        }

        return !string.IsNullOrWhiteSpace(segment.GetTarget());
    }
}
