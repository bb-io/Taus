using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class ReviewActionsTests : TestBase
{
    [TestMethod]
    public async Task ReviewContent_ValidXliff_Success()
    {
        var actions = new ReviewActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "About Us_en.html.xliff" };

        var result = await actions.EstimateContent(new ReviewContentRequest { File = file, Threshold = 0.8f });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ReviewContent_ValidXliff_12_Success()
    {
        var actions = new ReviewActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "xliff_Blackbird_Demo_File_en_de.xlf" };

        var result = await actions.EstimateContent(new ReviewContentRequest { File = file, Threshold = 0.7f });
        Assert.AreNotEqual(0, result.TotalSegmentsProcessed);

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ReviewContentContentful_ValidXliff_Success()
    {
        var actions = new ReviewActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "contentful.html.xliff" };

        var result = await actions.EstimateContent(new ReviewContentRequest { File = file, Threshold = 0.8f });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ReviewTextContent_Success()
    {
        var actions = new ReviewActions(InvocationContext, FileManager);
        var result = await actions.EstimateTextContent(new ReviewTextRequest { 
        SourceText="Hello",
        SourceLanguage="en",
        TargetText="Hola",
        TargetLanguage="es"});

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
