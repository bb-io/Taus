using System.Globalization;
using System.Xml.Linq;
using Apps.Taus.Models.Mxliff;

namespace Apps.Taus.Services.Mxliff;

/// <summary>
/// Parses MXLIFF (Memsource XLIFF) responses from TAUS Batch API.
/// Extracts quality scores, APE results, and billing information.
/// </summary>
public sealed class MxliffParser
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";
    private const string MemsourceNamespace = "http://www.memsource.com/mxlf/2.0";

    private static readonly XNamespace XliffNs = XliffNamespace;
    private static readonly XNamespace MemsourceNs = MemsourceNamespace;

    /// <summary>
    /// Parses MXLIFF content from TAUS batch job response.
    /// </summary>
    /// <param name="mxliffContent">The MXLIFF XML content</param>
    /// <param name="originalTargets">Optional dictionary of original target texts by segment ID</param>
    /// <returns>Dictionary of segment results indexed by segment ID</returns>
    public IReadOnlyDictionary<string, MxliffSegmentResult> Parse(
        string mxliffContent,
        IReadOnlyDictionary<string, string>? originalTargets = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mxliffContent);

        var document = XDocument.Parse(mxliffContent);
        var results = new Dictionary<string, MxliffSegmentResult>();

        var transUnits = document.Descendants(XliffNs + "trans-unit");

        foreach (var transUnit in transUnits)
        {
            var segmentId = transUnit.Attribute("id")?.Value;
            if (string.IsNullOrWhiteSpace(segmentId))
                continue;

            var result = ParseTransUnit(transUnit, segmentId, originalTargets);
            results[segmentId] = result;
        }

        return results;
    }

    private static MxliffSegmentResult ParseTransUnit(
        XElement transUnit,
        string segmentId,
        IReadOnlyDictionary<string, string>? originalTargets)
    {
        var target = transUnit.Element(XliffNs + "target")?.Value ?? string.Empty;
        var source = transUnit.Element(XliffNs + "source")?.Value ?? string.Empty;
        
        // Get original target from dictionary if provided
        var originalTarget = originalTargets?.GetValueOrDefault(segmentId) ?? target;

        // Check if segment was locked
        var wasLocked = transUnit.Attribute(MemsourceNs + "locked")?.Value == "true";

        // Parse latest-trans-props for quality score and processed-by flag
        var latestTransProps = transUnit.Element(MemsourceNs + "latest-trans-props");
        var qualityScore = ParseQualityScore(latestTransProps);
        var processedByTaus = ParseProcessedBy(latestTransProps);

        // Determine if APE was applied by comparing target with original
        var apeWasApplied = !string.Equals(target, originalTarget, StringComparison.Ordinal);
        var apeText = apeWasApplied ? target : null;

        // Calculate billing
        var billedCharacters = CalculateBilledCharacters(source, originalTarget);
        var billedWords = apeWasApplied ? CalculateBilledWords(apeText!) : 0;

        return new MxliffSegmentResult
        {
            SegmentId = segmentId,
            QualityScore = qualityScore,
            ApeText = apeText,
            OriginalTarget = originalTarget,
            ProcessedByTaus = processedByTaus,
            BilledCharacters = billedCharacters,
            BilledWords = billedWords,
            WasLocked = wasLocked
        };
    }

    private static float? ParseQualityScore(XElement? latestTransProps)
    {
        if (latestTransProps is null)
            return null;

        var qualityProp = latestTransProps
            .Elements(MemsourceNs + "trans-prop")
            .FirstOrDefault(e => e.Attribute("name")?.Value == "target-quality");

        if (qualityProp is null)
            return null;

        var scoreText = qualityProp.Value;
        if (float.TryParse(scoreText, NumberStyles.Float, CultureInfo.InvariantCulture, out var score))
            return score;

        return null;
    }

    private static bool ParseProcessedBy(XElement? latestTransProps)
    {
        if (latestTransProps is null)
            return false;

        var processedByProp = latestTransProps
            .Elements(MemsourceNs + "trans-prop")
            .FirstOrDefault(e => e.Attribute("name")?.Value == "processed-by");

        return processedByProp?.Value == "taus";
    }

    private static int CalculateBilledCharacters(string source, string target)
    {
        // QE billing is based on source + target character count
        return source.Length + target.Length;
    }

    private static int CalculateBilledWords(string text)
    {
        // APE billing is based on word count
        // Simple word splitting by whitespace
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}