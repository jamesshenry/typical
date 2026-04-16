using Typical.Core.Statistics;

namespace Typical.Core.Events;

public record GameStateUpdatedMessage(
    string TargetText,
    string UserInput,
    GameStatisticsSnapshot Statistics,
    bool IsOver
);

public record GameResetMessage(ModeSettings Settings);

public record TypingSettings
{
    public bool? Punctuation { get; set; }
    public bool? Numbers { get; set; }
    public GameMode Mode { get; set; } = GameMode.Quote;
    public string Language { get; set; } = "english";
}

public enum GameMode
{
    Quote,
}


public enum QuoteLength { All, Short, Medium, Long }
public record GrammarSettings(bool Punctuation = true, bool Numbers = false);
public union Words(int Count, GrammarSettings Grammar);
public union Time(TimeSpan Duration, GrammarSettings Grammar);
public union Quote(QuoteLength a = QuoteLength.All);
public union ModeSettings(Words words, Time time, Quote quote);
