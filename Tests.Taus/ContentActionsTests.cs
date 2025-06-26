using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class ContentActionsTests : TestBase
{
    [TestMethod]
    public async Task EstimateContent_ValidXliff_Success()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "About Us_en.html.xliff" };

        var result = await actions.EstimateContent(new EstimateContentRequest { File = file, Threshhold = 0.8 });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
