using Apps.Taus.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class XliffActionsTests : TestBase
{
    [TestMethod]
    public async Task EstimateContent_ValidXliff_Success()
    {
        var actions = new XliffActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "simple.xliff" };

        var result = await actions.EstimateXliff(new() { File = file, SourceLang = "en", TargetLang = "es" });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
