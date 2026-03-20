using Apps.Taus.Models.Request;
using Apps.Taus.Services.XliffBatch;

namespace Tests.Taus;

[TestClass]
public class XliffBatchParserTests
{
    private readonly XliffBatchParser _parser = new();

    [TestMethod]
    public void Parse_ScoreOnly_UsesScoreAsEffectiveScore()
    {
        var result = ParseSingle(@"
<trans-unit id='1'>
  <source>Source</source>
  <target>Target</target>
  <prop-group name='epic-metadata'>
    <prop prop-type='score'>0.73156</prop>
    <prop prop-type='billed-characters'>57</prop>
  </prop-group>
</trans-unit>");

        Assert.AreEqual(0.73156f, result.Score);
        Assert.IsNull(result.ApeScore);
        Assert.AreEqual(0.73156f, result.EffectiveScore);
        Assert.IsNull(result.ApeResult);
        Assert.IsNull(result.Remarks);
        Assert.AreEqual(57, result.BilledCharacters);
        Assert.AreEqual(0, result.BilledWords);
    }

    [TestMethod]
    public void Parse_ApeMetadata_UsesApeScoreAndCapturesRemarks()
    {
        var result = ParseSingle(@"
<trans-unit id='2'>
  <source>Source</source>
  <target>Target</target>
  <prop-group name='epic-metadata'>
    <prop prop-type='score'>0.71289</prop>
    <prop prop-type='ape-result'>Bitte wenden Sie sich an den Support.</prop>
    <prop prop-type='ape-score'>0.89634</prop>
    <prop prop-type='remarks'>More natural phrasing in German.</prop>
    <prop prop-type='billed-characters'>0</prop>
    <prop prop-type='billed-words'>5</prop>
  </prop-group>
</trans-unit>");

        Assert.AreEqual(0.71289f, result.Score);
        Assert.AreEqual(0.89634f, result.ApeScore);
        Assert.AreEqual(0.89634f, result.EffectiveScore);
        Assert.AreEqual("Bitte wenden Sie sich an den Support.", result.ApeResult);
        Assert.AreEqual("More natural phrasing in German.", result.Remarks);
        Assert.AreEqual(0, result.BilledCharacters);
        Assert.AreEqual(5, result.BilledWords);
    }

    [TestMethod]
    public void BackgroundDownloadRequest_DefaultsToAddingLowScoreComment()
    {
        var request = new BackgroundDownloadRequest();

        Assert.IsTrue(request.AddLowScoreEditedByTausComment);
    }

    private Apps.Taus.Models.XliffBatch.XliffBatchSegmentResult ParseSingle(string transUnit)
    {
        var document = $$"""
<?xml version='1.0' encoding='UTF-8'?>
<xliff xmlns='urn:oasis:names:tc:xliff:document:1.2' version='1.2'>
  <file original='example.html' source-language='en' target-language='de' datatype='plaintext'>
    <body>
      {{transUnit}}
    </body>
  </file>
</xliff>
""";

        return _parser.Parse(document).Single().Value;
    }
}
