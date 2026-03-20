namespace Apps.Taus.Services.XliffBatch;

internal sealed class SegmentIdGenerator(string fileName)
{
    private readonly string _prefix = Path.GetFileNameWithoutExtension(fileName) ?? "file";
    private int _counter = 1;

    public string GetNextId()
    {
        return $"{_prefix}-{_counter++}";
    }
}
