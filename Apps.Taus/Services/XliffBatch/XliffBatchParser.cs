using System.Globalization;
using System.Xml.Linq;
using Apps.Taus.Models.XliffBatch;

namespace Apps.Taus.Services.XliffBatch;

public sealed class XliffBatchParser
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";

    private static readonly XNamespace XliffNs = XliffNamespace;

    public IReadOnlyDictionary<string, XliffBatchSegmentResult> Parse(string xliffContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xliffContent);

        var document = XDocument.Parse(xliffContent);
        var results = new Dictionary<string, XliffBatchSegmentResult>();

        foreach (var transUnit in document.Descendants(XliffNs + "trans-unit"))
        {
            var segmentId = transUnit.Attribute("id")?.Value;
            if (string.IsNullOrWhiteSpace(segmentId))
                continue;

            results[segmentId] = ParseTransUnit(transUnit, segmentId);
        }

        return results;
    }

    private static XliffBatchSegmentResult ParseTransUnit(XElement transUnit, string segmentId)
    {
        var metadata = transUnit
            .Elements(XliffNs + "prop-group")
            .FirstOrDefault(x => x.Attribute("name")?.Value == "epic-metadata");

        return new XliffBatchSegmentResult
        {
            SegmentId = segmentId,
            Score = ParseFloatProp(metadata, "score"),
            ApeScore = ParseFloatProp(metadata, "ape-score"),
            ApeResult = ParseStringProp(metadata, "ape-result"),
            Remarks = ParseStringProp(metadata, "remarks"),
            BilledCharacters = ParseIntProp(metadata, "billed-characters"),
            BilledWords = ParseIntProp(metadata, "billed-words")
        };
    }

    private static string? ParseStringProp(XElement? metadata, string propType)
    {
        return metadata?
            .Elements(XliffNs + "prop")
            .FirstOrDefault(x => x.Attribute("prop-type")?.Value == propType)?
            .Value;
    }

    private static float? ParseFloatProp(XElement? metadata, string propType)
    {
        var value = ParseStringProp(metadata, propType);
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var score)
            ? score
            : null;
    }

    private static int ParseIntProp(XElement? metadata, string propType)
    {
        var value = ParseStringProp(metadata, propType);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}
