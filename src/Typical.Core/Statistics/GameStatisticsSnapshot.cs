namespace Typical.Core.Statistics;

public record GameStatisticsSnapshot(
    double WordsPerMinute,
    double Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime,
    bool IsRunning
)
{
    public static GameStatisticsSnapshot Empty =>
        new(0, 100, new CharacterStats(0, 0, 0, 0), TimeSpan.Zero, false);
}
