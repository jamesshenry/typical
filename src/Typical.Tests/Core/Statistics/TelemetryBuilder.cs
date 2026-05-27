using System.Globalization;
using Microsoft.Extensions.Time.Testing;
using Typical.Core.Statistics;

namespace Typical.Tests.Core.Statistics;

public class TelemetryBuilder
{
    private readonly List<KeystrokeLog> _logs = new();
    private int _currentIndex = 0;
    private readonly TestSession _stats;
    private readonly FakeTimeProvider _time;

    public TelemetryBuilder(TestSession stats, FakeTimeProvider fakeTime)
    {
        _stats = stats;
        _time = fakeTime;
    }

    public TelemetryBuilder Type(string text, int delayMs = 0)
    {
        foreach (var grapheme in text.EnumerateRunes()) // Simplification for test
        {
            _stats.RecordKey(grapheme.ToString(), KeystrokeType.Correct, _currentIndex++);
            if (delayMs > 0)
                _time.Advance(TimeSpan.FromMilliseconds(delayMs));
        }
        return this;
    }

    /// <summary>
    /// Simulates a mistake (Incorrect key).
    /// </summary>
    public TelemetryBuilder Error(string actualGrapheme, int delayMs = 0)
    {
        // On an error, we record the key at the CURRENT index,
        // but we do NOT increment _currentIndex.
        _stats.RecordKey(actualGrapheme, KeystrokeType.Incorrect, _currentIndex);

        if (delayMs > 0)
            _time.Advance(TimeSpan.FromMilliseconds(delayMs));

        return this;
    }

    /// <summary>
    /// Simulates hitting backspace.
    /// </summary>
    public TelemetryBuilder Backspace(int delayMs = 0)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            _stats.RecordBackspace(_currentIndex);
        }

        if (delayMs > 0)
            _time.Advance(TimeSpan.FromMilliseconds(delayMs));

        return this;
    }

    public TelemetryBuilder Advance(TimeSpan duration)
    {
        _time.Advance(duration);
        return this;
    }
}
