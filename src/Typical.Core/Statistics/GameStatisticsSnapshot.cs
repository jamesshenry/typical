using Vogen;

namespace Typical.Core.Statistics;

public record GameStatisticsSnapshot(
    double WordsPerMinute,
    Accuracy Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime,
    bool IsRunning
)
{
    public static GameStatisticsSnapshot Empty =>
        new(0, Accuracy.From(100), new CharacterStats(0, 0, 0, 0), TimeSpan.Zero, false);
}

[ValueObject<double>]
public partial struct Accuracy
{
    public override string ToString()
    {
        return $"{Value:F1}";
    }
}
