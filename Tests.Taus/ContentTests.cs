using Apps.Taus.Actions;
using Apps.Taus.Connections;
using Apps.Taus.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Taus.Base;

namespace Tests.Taus;

[TestClass]
public class ContentTests : TestBase
{
    [TestMethod]
    public async Task Review_xliff_content()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "contentful.html.xliff" };

        var result = await actions.EstimateContent(new EstimateContentRequest { File = file, Threshhold = 0.8 });

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsTrue(result.TotalSegmentsProcessed > 0);
    }
}
