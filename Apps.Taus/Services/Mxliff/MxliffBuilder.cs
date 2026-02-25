using System.Text;
using System.Xml;
using Apps.Taus.Models.Mxliff;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Transformations;

namespace Apps.Taus.Services.Mxliff;

/// <summary>
/// Custom StringWriter that uses UTF-8 encoding instead of UTF-16
/// </summary>
internal sealed class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

/// <summary>
/// Builds MXLIFF (Memsource XLIFF) documents from Transformation objects.
/// Follows MXLIFF 2.0 specification with support for segment filtering and ID generation.
/// </summary>
public sealed class MxliffBuilder
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";
    private const string MemsourceNamespace = "http://www.memsource.com/mxlf/2.0";

    /// <summary>
    /// Builds an MXLIFF document from a Transformation object.
    /// </summary>
    /// <param name="transformation">The transformation containing segments to process</param>
    /// <param name="segmentStatesToInclude">States of segments to include (default: Initial, Translated)</param>
    /// <param name="segmentStateQualifiersToExclude">State qualifiers to exclude segments</param>
    /// <returns>MXLIFF XML string and segment ID mappings</returns>
    public (string MxliffContent, IReadOnlyList<SegmentIdMapping> IdMappings) Build(
        Transformation transformation,
        IEnumerable<SegmentState>? segmentStatesToInclude = null,
        IEnumerable<string>? segmentStateQualifiersToExclude = null)
    {
        ArgumentNullException.ThrowIfNull(transformation);

        var statesToInclude = segmentStatesToInclude?.ToList() ?? new List<SegmentState> { SegmentState.Initial, SegmentState.Translated };
        var qualifiersToExclude = segmentStateQualifiersToExclude?.ToList() ?? new List<string>();

        var idGenerator = new SegmentIdGenerator(transformation.OriginalName ?? "file");
        var idMappings = new List<SegmentIdMapping>();

        var settings = new XmlWriterSettings
        {
            Indent = false,
            Encoding = new UTF8Encoding(false),
            OmitXmlDeclaration = false
        };

        using var stringWriter = new Utf8StringWriter();
        using var writer = XmlWriter.Create(stringWriter, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("xliff", XliffNamespace);
        writer.WriteAttributeString("xmlns", "m", null, MemsourceNamespace);
        writer.WriteAttributeString("version", "1.2");

        writer.WriteStartElement("file");
        writer.WriteAttributeString("original", transformation.OriginalName);
        writer.WriteAttributeString("source-language", transformation.SourceLanguage ?? "en");
        writer.WriteAttributeString("target-language", transformation.TargetLanguage ?? "en");

        writer.WriteStartElement("body");

        var groupId = 0;
        var sequenceIndex = 0;

        foreach (var unit in transformation.GetUnits())
        {
            // Skip units that are already reviewed or finalized
            if (unit.State == SegmentState.Reviewed || unit.State == SegmentState.Final)
                continue;

            foreach (var segment in unit.Segments)
            {
                if (!ShouldProcessSegment(segment, statesToInclude, qualifiersToExclude))
                    continue;

                var segmentId = GetOrGenerateSegmentId(segment, idGenerator, sequenceIndex, out var mapping);
                idMappings.Add(mapping);

                WriteTransUnit(writer, segmentId, segment, groupId);

                groupId++;
                sequenceIndex++;
            }
        }

        writer.WriteEndElement(); // body
        writer.WriteEndElement(); // file
        writer.WriteEndElement(); // xliff
        writer.WriteEndDocument();
        writer.Flush();

        return (stringWriter.ToString(), idMappings);
    }

    private static bool ShouldProcessSegment(
        Segment segment,
        IReadOnlyList<SegmentState> statesToInclude,
        IReadOnlyList<string> qualifiersToExclude)
    {
        // Skip ignorable segments
        if (segment.IsIgnorbale)
            return false;

        // Check if segment state is in the list of states to estimate
        var segmentState = segment.State ?? SegmentState.Initial;
        if (!statesToInclude.Contains(segmentState))
            return false;

        // Check state qualifiers exclusion
        if (qualifiersToExclude.Any())
        {
            var stateQualifier = segment.TargetAttributes.FirstOrDefault(a => a.Name == "state-qualifier");
            if (stateQualifier is not null && qualifiersToExclude.Contains(stateQualifier.Value))
                return false;
        }

        // Skip segments without target text
        if (string.IsNullOrWhiteSpace(segment.GetTarget()))
            return false;

        return true;
    }

    private static string GetOrGenerateSegmentId(
        Segment segment,
        SegmentIdGenerator idGenerator,
        int sequenceIndex,
        out SegmentIdMapping mapping)
    {
        var originalId = segment.Id ?? string.Empty;
        var hasId = !string.IsNullOrWhiteSpace(originalId);

        var mxliffId = hasId ? originalId : idGenerator.GetNextId();

        mapping = new SegmentIdMapping
        {
            OriginalId = originalId,
            MxliffId = mxliffId,
            IsGenerated = !hasId,
            SequenceIndex = sequenceIndex
        };

        return mxliffId;
    }

    private static void WriteTransUnit(XmlWriter writer, string id, Segment segment, int groupId)
    {
        writer.WriteStartElement("group");
        writer.WriteAttributeString("id", groupId.ToString());

        writer.WriteStartElement("trans-unit");
        writer.WriteAttributeString("id", id);
        writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");

        writer.WriteStartElement("source");
        writer.WriteString(segment.GetSource());
        writer.WriteEndElement(); // source

        writer.WriteStartElement("target");
        writer.WriteString(segment.GetTarget());
        writer.WriteEndElement(); // target

        writer.WriteEndElement(); // trans-unit
        writer.WriteEndElement(); // group
    }
}
