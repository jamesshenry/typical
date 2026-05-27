using System.Diagnostics;
using Vogen;

namespace Typical.Core.Statistics;

public readonly record struct TestSnapshot(
    WPM WPM,
    Accuracy Accuracy,
    TestMetrics Chars,
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
            WPM.From(Math.Max(0, wpmValue)),
            Accuracy.From(Math.Clamp(accValue, 0, 100)),
            chars,
            elapsed
        );
        Debug.WriteLine(snapshot);
        return snapshot;
    }

    public static TestSnapshot Empty =>
        new((WPM)0, (Accuracy)100, new TestMetrics(0, 0, 0), TimeSpan.Zero);
}

[ValueObject<double>]
public partial struct Accuracy
{
    public override string ToString()
    {
        return $"{Value:F1}";
    }
}

[ValueObject<double>]
public partial struct WPM
{
    public override string ToString()
    {
        return $"{Value:F1}";
    }
}
