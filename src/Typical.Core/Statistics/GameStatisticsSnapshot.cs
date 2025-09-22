namespace Typical.Core.Statistics;

public record GameStatisticsSnapshot(
    double WordsPerMinute,
    double Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime,
    bool IsRunning
);
