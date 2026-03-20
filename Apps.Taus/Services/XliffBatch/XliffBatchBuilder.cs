using System.Text;
using System.Xml;
using Apps.Taus.Models.XliffBatch;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Transformations;

namespace Apps.Taus.Services.XliffBatch;

public sealed class XliffBatchBuilder
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";

    public (string XliffContent, IReadOnlyList<SegmentIdMapping> IdMappings) Build(
        Transformation transformation,
        IEnumerable<SegmentState>? segmentStatesToInclude = null,
        IEnumerable<string>? segmentStateQualifiersToExclude = null)
    {
        ArgumentNullException.ThrowIfNull(transformation);

        var statesToInclude = segmentStatesToInclude?.ToList() ?? [SegmentState.Initial, SegmentState.Translated];
        var qualifiersToExclude = segmentStateQualifiersToExclude?.ToList() ?? [];
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
        writer.WriteAttributeString("version", "1.2");

        writer.WriteStartElement("file");
        writer.WriteAttributeString("original", transformation.OriginalName);
        writer.WriteAttributeString("source-language", transformation.SourceLanguage ?? "en");
        writer.WriteAttributeString("target-language", transformation.TargetLanguage ?? "en");
        writer.WriteAttributeString("datatype", "plaintext");

        writer.WriteStartElement("body");

        var groupId = 0;
        var sequenceIndex = 0;

        foreach (var unit in transformation.GetUnits())
        {
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

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        return (stringWriter.ToString(), idMappings);
    }

    private static bool ShouldProcessSegment(
        Segment segment,
        IReadOnlyList<SegmentState> statesToInclude,
        IReadOnlyList<string> qualifiersToExclude)
    {
        if (segment.IsIgnorbale)
            return false;

        var segmentState = segment.State ?? SegmentState.Initial;
        if (!statesToInclude.Contains(segmentState))
            return false;

        if (qualifiersToExclude.Any())
        {
            var stateQualifier = segment.TargetAttributes.FirstOrDefault(a => a.Name == "state-qualifier");
            if (stateQualifier is not null && qualifiersToExclude.Contains(stateQualifier.Value))
                return false;
        }

        return !string.IsNullOrWhiteSpace(segment.GetTarget());
    }

    private static string GetOrGenerateSegmentId(
        Segment segment,
        SegmentIdGenerator idGenerator,
        int sequenceIndex,
        out SegmentIdMapping mapping)
    {
        var originalId = segment.Id ?? string.Empty;
        var hasId = !string.IsNullOrWhiteSpace(originalId);
        var batchSegmentId = hasId ? originalId : idGenerator.GetNextId();

        mapping = new SegmentIdMapping
        {
            OriginalId = originalId,
            BatchSegmentId = batchSegmentId,
            IsGenerated = !hasId,
            SequenceIndex = sequenceIndex
        };

        return batchSegmentId;
    }

    private static void WriteTransUnit(XmlWriter writer, string id, Segment segment, int groupId)
    {
        writer.WriteStartElement("group");
        writer.WriteAttributeString("id", groupId.ToString());

        writer.WriteStartElement("trans-unit");
        writer.WriteAttributeString("id", id);

        writer.WriteStartElement("source");
        writer.WriteString(segment.GetSource());
        writer.WriteEndElement();

        writer.WriteStartElement("target");
        writer.WriteString(segment.GetTarget());
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}
