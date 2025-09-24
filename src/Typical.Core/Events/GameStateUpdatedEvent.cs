using Typical.Core.Statistics;

namespace Typical.Core.Events;

public record GameStateUpdatedEvent(
    string TargetText,
    string UserInput,
    GameStatisticsSnapshot Statistics,
    bool IsOver
);
