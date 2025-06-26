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
        var file = new FileReference { Name = "About Us_en.html.xliff" };

        var result = await actions.EstimateXliff(new()
        {
            File = file,
            SourceLang = "en",
            TargetLang = "es",
            Threshold = [0.8],
            Condition = [">="],
            State = ["reviewed"]
        });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
