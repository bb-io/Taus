using Apps.Taus.Services.SegmentProcessing;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Transformations;

namespace Tests.Taus;

[TestClass]
public class SegmentProcessingHelperTests
{
    [TestMethod]
    public void ShouldProcessSegment_WithoutStateFilter_AllowsReviewedSegment()
    {
        var segment = CreateSegment("reviewed", null, "Translated text");

        var result = SegmentProcessingHelper.ShouldProcessSegment(segment, qualifiersToExclude: Array.Empty<string>());

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldProcessSegment_WithExcludedQualifier_SkipsSegment()
    {
        var segment = CreateSegment("translated", "leveraged-tm", "Translated text");

        var result = SegmentProcessingHelper.ShouldProcessSegment(segment, qualifiersToExclude: ["leveraged-tm"]);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldProcessSegment_WithStateFilter_ExcludesReviewedSegment()
    {
        var segment = CreateSegment("reviewed", null, "Translated text");

        var result = SegmentProcessingHelper.ShouldProcessSegment(
            segment,
            [SegmentState.Initial, SegmentState.Translated],
            Array.Empty<string>());

        Assert.IsFalse(result);
    }

    private static Segment CreateSegment(string state, string? qualifier, string target)
    {
        var qualifierAttribute = qualifier is null ? string.Empty : $" state-qualifier=\"{qualifier}\"";
        var transformation = Transformation.Parse($$"""
<?xml version="1.0" encoding="UTF-8"?>
<xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
  <file original="sample.html" source-language="en" target-language="de" datatype="plaintext">
    <body>
      <trans-unit id="1">
        <source>Source</source>
        <target state="{{state}}"{{qualifierAttribute}}>{{target}}</target>
      </trans-unit>
    </body>
  </file>
</xliff>
""", "sample.xlf");

        return transformation!.GetUnits().Single().Segments.Single();
    }
}
