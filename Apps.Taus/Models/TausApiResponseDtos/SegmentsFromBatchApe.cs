using Blackbird.Applications.Sdk.Common.Exceptions;
using System.Globalization;

namespace Apps.Taus.Models.TausApiResponseDtos;

public record SegmentsFromBatchApe(
    int Index,
    string Source,
    string Target,
    double Score,
    string? ApeResult,
    string? Remarks,
    double? ApeScore,
    int? BilledCharacters,
    int? BilledWords
)
{
    public static SegmentsFromBatchApe FromArray(string[] cols)
    {
        double? ParseDouble(string val) => double.TryParse(val, CultureInfo.InvariantCulture, out var res) ? res : null;
        int? ParseInt(string? val) => int.TryParse(val, out var res) ? res : null;

        var index = ParseInt(cols.ElementAtOrDefault(0)) ?? 0;
        var source = cols.ElementAtOrDefault(1) ?? "";
        var target = cols.ElementAtOrDefault(2) ?? "";

        // Only estimation was done and APE didn't kick off
        if (cols.Length == 6)
        {
            return new SegmentsFromBatchApe(
                Index: index,
                Source: source,
                Target: target,
                Score: ParseDouble(cols[3]) ?? 0,
                ApeResult: null,
                Remarks: null,
                ApeScore: null,
                BilledCharacters: ParseInt(cols[4]),
                BilledWords: ParseInt(cols.ElementAtOrDefault(5)));
        }

        // APE was applied
        if (cols.Length == 9)
        {
            return new SegmentsFromBatchApe(
                Index: index,
                Source: source,
                Target: target,
                Score: ParseDouble(cols.ElementAtOrDefault(3) ?? "") ?? 0,
                ApeResult: cols.ElementAtOrDefault(4),
                Remarks: cols.ElementAtOrDefault(5),
                ApeScore: ParseDouble(cols.ElementAtOrDefault(6) ?? ""),
                BilledCharacters: ParseInt(cols.ElementAtOrDefault(7)),
                BilledWords: ParseInt(cols.ElementAtOrDefault(8)));
        }

        throw new PluginApplicationException($"TAUS has returned an unexpected number of columns ({cols.Length}).");
    }
};
