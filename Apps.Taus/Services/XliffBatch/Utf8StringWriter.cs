using System.Text;

namespace Apps.Taus.Services.XliffBatch;

internal sealed class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}
