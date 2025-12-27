using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class EditActionsTests : TestBase
{
    [TestMethod]
    public async Task EditContent_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "About Us_en.html.xliff" };

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8 });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EditContentContentful_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "contentful.html.xliff" };

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8 });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EditContentXtm_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "xliff-after-xtm.xlf" };

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8, OutputFileHandling = "original" });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EditTextContent_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var result = await actions.EditText(new EditTextRequest { 
        SourceText="The Loirevalley",
        SourceLanguage="en",
        TargetText="De Loiredal",
        TargetLanguage="nl"});

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
