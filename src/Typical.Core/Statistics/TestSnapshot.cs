using System.Diagnostics;

namespace Typical.Core.Statistics;

public readonly record struct TestSnapshot(
    Wpm WPM,
    Accuracy Accuracy,
    TestMetrics Metrics,
    TimeSpan ElapsedTime
)
{
    public static TestSnapshot Create(TestMetrics chars, TimeSpan elapsed)
    {
        int totalTyped = chars.Correct + chars.Corrections + chars.Incorrect;
        double accValue = totalTyped == 0 ? 100.0 : (double)chars.Correct / totalTyped * 100.0;
        double minutes = elapsed.TotalMinutes;
        double wpmValue = (minutes <= 0) ? 0 : (chars.Correct / 5.0) / minutes;

        var snapshot = new TestSnapshot(
            Wpm.From(Math.Max(0, wpmValue)),
            Accuracy.From(Math.Clamp(accValue, 0, 100)),
            chars,
            elapsed
        );
        Debug.WriteLine(snapshot);
        return snapshot;
    }

    public static TestSnapshot Empty =>
        new((Wpm)0, (Accuracy)100, new TestMetrics(0, 0, 0), TimeSpan.Zero);
}
