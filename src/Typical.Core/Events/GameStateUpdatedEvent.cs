using Typical.Core.Statistics;

namespace Typical.Core.Events;

public record GameStateUpdatedMessage(
    string TargetText,
    string UserInput,
    GameStatisticsSnapshot Statistics,
    bool IsOver
);

public record GameResetMessage(ModeSettings Settings);

public record WordsMode(int Count, bool Punctuation, bool Numbers);
public record TimeMode(TimeSpan Duration, bool Punctuation, bool Numbers);
public record QuoteMode(QuoteLength Length);
public record ZenMode; // Empty marker record

public enum QuoteLength { All, Short, Medium, Long }

public union ModeSettings(WordsMode, TimeMode, QuoteMode, ZenMode);
