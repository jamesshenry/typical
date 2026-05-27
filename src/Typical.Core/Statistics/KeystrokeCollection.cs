using System.Diagnostics;

namespace Typical.Core.Statistics;

public class KeystrokeCollection
{
    private readonly List<KeystrokeLog> _logs = new();
    private long? _firstTimestamp;

    public int CorrectCount { get; private set; }
    public int TotalPhysical => _logs.Count;
    public int ErrorCount { get; private set; }
    public int CorrectionCount { get; private set; }

    /// <param name="actual">The grapheme typed.</param>
    /// <param name="type">The type of keystroke.</param>
    /// <param name="timestamp">Raw timestamp from TimeProvider.</param>
    /// <param name="index">The current grapheme index in the target text.</param>
    public void Add(string actual, KeystrokeType type, long timestamp, int index)
    {
        // Set the baseline for OffsetMs on the very first keypress
        _firstTimestamp ??= timestamp;

        // Calculate offset in milliseconds (for SQLite efficiency)
        // We use a helper or manual math here depending on your TimeProvider resolution
        long offsetMs = (timestamp - _firstTimestamp.Value) / TimeSpan.TicksPerMillisecond;

        var log = new KeystrokeLog(
            Value: actual,
            Type: type,
            Timestamp: timestamp,
            OffsetMs: offsetMs,
            Index: index
        );

        _logs.Add(log);

        switch (type)
        {
            case KeystrokeType.Correct:
                CorrectCount++;
                break;
            case KeystrokeType.Incorrect:
                ErrorCount++;
                break;
            case KeystrokeType.Correction:
                CorrectionCount++;
                break;
        }

        LogDebug(log);
    }

    internal void Clear()
    {
        _logs.Clear();
        _firstTimestamp = null;
        CorrectCount = 0;
        ErrorCount = 0;
        CorrectionCount = 0;
    }

    internal IReadOnlyList<KeystrokeLog> GetLog() => _logs.AsReadOnly();

    [Conditional("DEBUG")]
    private void LogDebug(KeystrokeLog log) => Debug.WriteLine(log);
}
