using Apps.Taus.Actions;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common.Exceptions;
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
        var file = new FileReference { Name = "2.1-deepL.xliff.xliff" };

        var result = await actions.EstimateXliff(new()
        {
            File = file,
            SourceLang = "uk",
            TargetLang = "es",
            //Threshold = [0.8],
            //Condition = [">="],
            //State = ["reviewed"]
        });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EstimateContent_NonXliffFile_ThrowsMisconfigException()
    {
        // Arrange
        var actions = new XliffActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "2-2025 payoneer.pdf", ContentType = "application/pdf" };
        var input = new EstimateXliffInput { File = file, TargetLang = "en" };

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(
            async () => await actions.EstimateXliff(input)
        );

        // Assert
        StringAssert.Contains(ex.Message, "Expected an XLIFF file");
    }
}
