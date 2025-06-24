using Apps.Taus.Models;
using Blackbird.Xliff.Utils;

namespace Apps.Taus.Utils;

public static class XliffToCsvConverter
{
    private static readonly CsvService _csvService = new CsvService();
    
    public static async Task<Stream> ConvertXliffToCsv(XliffDocument xliffDocument)
    {
        var keyValuePairs = xliffDocument.Files
            .SelectMany(file => file.TranslationUnits)
            .Where(unit => unit.Source != null)
            .Select(unit => new KeyValuePairEntity(unit.Source.Content!, unit.Target?.Content ?? string.Empty))
            .ToList();

        var memoryStream = new MemoryStream();
        await _csvService.WriteToCsvAsync(keyValuePairs, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
