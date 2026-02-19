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

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8f });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EditContentContentful_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "contentful.html.xliff" };

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8f });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EditContentXtm_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "xliff-after-xtm.xlf" };

        var result = await actions.EditContent(new EditContentRequest { File = file, Threshold = 0.8f, OutputFileHandling = "original" });

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

    [TestMethod]
    public async Task EditContentInBackground_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var request = new EditContentInBackgroundRequest {
            Files = [new FileReference { Name = "xliff-after-xtm.xlf" }],
            Threshold = 0.8f,
        };

        var result = await actions.EditContentInBackground(request);

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.JobIds.Any());
        Assert.IsFalse(result.JobCreationErrors.Any());
    }

    [TestMethod]
    public async Task DownloadContentFromBackground_ValidXliff_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var request = new BackgroundDownloadRequest
        {
            JobIds = ["d31cd848-5e14-4e7b-9cfb-81dfbfcbcd24"],
            TransformationFiles = [new FileReference { Name = "background-transformation.xlf" }],
        };

        var result = await actions.DownloadContentFromBackground(request);

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.ProcessedFiles.Any());
    }

    [TestMethod]
    public async Task DownloadContentFromBackground_ValidXliff_Xliff1Output_Success()
    {
        var actions = new EditActions(InvocationContext, FileManager);
        var request = new BackgroundDownloadRequest
        {
            JobIds = ["d31cd848-5e14-4e7b-9cfb-81dfbfcbcd24"],
            TransformationFiles = [new FileReference { Name = "background-transformation.xlf" }],
            OutputFileHandling = "xliff1",
        };

        var result = await actions.DownloadContentFromBackground(request);

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.ProcessedFiles.Any());
    }
}
