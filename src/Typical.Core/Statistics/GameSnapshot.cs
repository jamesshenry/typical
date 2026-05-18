using System.Diagnostics;
using Vogen;

namespace Typical.Core.Statistics;

public readonly record struct GameSnapshot(
    WPM WPM,
    Accuracy Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime
)
{
    public static GameSnapshot Create(int correct, int totalTyped, int errors, TimeSpan elapsed)
    {
        // 1. Calculate Accuracy
        double accValue = totalTyped == 0 ? 100.0 : (double)correct / totalTyped * 100.0;

        // 2. Calculate WPM (Standard: 5 chars = 1 word)
        // Note: Using totalTyped for 'Raw' WPM or correctChars for 'Net' WPM
        double minutes = elapsed.TotalMinutes;
        double wpmValue = (minutes <= 0) ? 0 : (correct / 5.0) / minutes;
        var snapshot = new GameSnapshot(
            WPM.From(Math.Max(0, wpmValue)),
            Accuracy.From(Math.Clamp(accValue, 0, 100)),
            new CharacterStats(correct, totalTyped, errors),
            elapsed
        );
        Debug.WriteLine(snapshot);
        return snapshot;
    }

    public static GameSnapshot Empty =>
        new((WPM)0, (Accuracy)100, new CharacterStats(0, 0, 0), TimeSpan.Zero);
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
