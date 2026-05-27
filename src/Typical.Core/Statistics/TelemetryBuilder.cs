using System.Globalization;

namespace Typical.Core.Statistics;

public class TelemetryBuilder
{
    private readonly List<KeystrokeLog> _logs = new();
    private int _currentIndex = 0;
    private long _currentOffset = 0;

    public TelemetryBuilder Type(string text, int delayMs = 100)
    {
        // Use StringInfo to iterate by Grapheme, not by Char!
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string grapheme = enumerator.GetTextElement();

            _logs.Add(
                new KeystrokeLog(
                    Value: grapheme,
                    Type: KeystrokeType.Correct,
                    Timestamp: 0,
                    OffsetMs: _currentOffset += delayMs,
                    Index: _currentIndex++
                )
            );
        }
        return this;
    }

    public TelemetryBuilder Error(string actualGrapheme, int delayMs = 100)
    {
        _logs.Add(
            new KeystrokeLog(
                Value: actualGrapheme,
                Type: KeystrokeType.Incorrect,
                Timestamp: 0,
                OffsetMs: _currentOffset += delayMs,
                Index: _currentIndex // Pointer doesn't move on error
            )
        );
        return this;
    }

    public List<KeystrokeLog> Build() => _logs;
}
