using System.Diagnostics;

namespace Typical.Core.Statistics;

public class KeystrokeCollection
{
    private readonly List<KeystrokeLog> _logs = new();

    public int CorrectCount { get; private set; }
    public int TotalPhysicalKeystrokes => _logs.Count;
    public int ErrorCount { get; private set; }
    public int CorrectionCount { get; private set; }

    public void Add(string actual, KeystrokeType type, long timestamp)
    {
        var log = new KeystrokeLog(actual, type, timestamp);
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
    }

    internal IReadOnlyList<KeystrokeLog> GetLog()
    {
        return _logs.AsReadOnly();
    }

    [Conditional("DEBUG")]
    private void LogDebug(KeystrokeLog log) => Debug.WriteLine(log);
}
