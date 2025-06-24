using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using Apps.Taus.Models;

namespace Apps.Taus.Utils;

public class CsvService
{
    private readonly CsvConfiguration _csvConfiguration;

    public CsvService()
    {
        _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Quote = '"',
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null
        };
    }
    
    public async Task WriteToCsvAsync(IEnumerable<KeyValuePairEntity> KeyValuePairEntitys, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(KeyValuePairEntitys);
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable", nameof(stream));

        using var writer = new StreamWriter(stream, leaveOpen: true);
        using var csv = new CsvWriter(writer, _csvConfiguration);

        // Write header
        csv.WriteField("Key");
        csv.WriteField("Value");
        await csv.NextRecordAsync();

        foreach (var kvp in KeyValuePairEntitys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(kvp.Key);
            csv.WriteField(kvp.Value);
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync(cancellationToken);
    }

    public async Task<List<KeyValuePairEntity>> ReadFromCsvAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(stream));

        var result = new List<KeyValuePairEntity>();

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, _csvConfiguration);

        // Read header
        await csv.ReadAsync();
        csv.ReadHeader();

        var headerRecord = csv.HeaderRecord;
        if (headerRecord == null || headerRecord.Length < 2)
        {
            throw new InvalidOperationException("CSV file must have at least 2 columns with headers");
        }

        var keyIndex = csv.GetFieldIndex("Key");
        var valueIndex = csv.GetFieldIndex("Value");

        if (keyIndex == -1 || valueIndex == -1)
        {
            throw new InvalidOperationException("CSV file must contain 'Key' and 'Value' columns");
        }

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = csv.GetField<string>(keyIndex) ?? string.Empty;
            var value = csv.GetField<string>(valueIndex) ?? string.Empty;

            result.Add(new KeyValuePairEntity(key, value));
        }

        return result;
    }
}
