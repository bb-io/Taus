namespace Apps.Taus.Services.Mxliff;

/// <summary>
/// Generates unique segment IDs for segments that don't have an ID.
/// Uses a deterministic approach based on original filename and sequence.
/// </summary>
public sealed class SegmentIdGenerator
{
    private readonly string _filePrefix;
    private int _counter;

    public SegmentIdGenerator(string fileName)
    {
        // Generate a simple prefix from filename
        _filePrefix = SanitizeFileName(fileName);
        _counter = 0;
    }

    /// <summary>
    /// Gets the next unique ID for a segment
    /// </summary>
    public string GetNextId() => $"{_filePrefix}_seg_{_counter++}";

    /// <summary>
    /// Resets the counter (useful for testing)
    /// </summary>
    public void Reset() => _counter = 0;

    private static string SanitizeFileName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        
        // Remove special characters and limit length
        var sanitized = new string(nameWithoutExtension
            .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
            .Take(20)
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "segment" : sanitized;
    }
}
