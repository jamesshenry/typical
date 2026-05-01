using Vogen;

namespace Typical.Core.Statistics;

public readonly record struct GameSnapshot(
    double WordsPerMinute,
    Accuracy Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime,
    bool IsRunning,
    string TargetText,
    string UserInput,
    bool IsOver
)
{
    public static GameSnapshot Empty =>
        new(
            0,
            Accuracy.From(100),
            new CharacterStats(0, 0, 0, 0),
            TimeSpan.Zero,
            false,
            "",
            "",
            true
        );
}

[ValueObject<double>]
public partial struct Accuracy
{
    public override string ToString()
    {
        return $"{Value:F1}";
    }
}
