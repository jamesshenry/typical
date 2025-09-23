using Typical.Core.Statistics;

namespace Typical.Core.Events;

public class GameEndedEventArgs : EventArgs
{
    public GameEndedEventArgs(GameStatisticsSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public GameStatisticsSnapshot Snapshot { get; }
}

public class GameStateChangedEventArgs : EventArgs
{
    // You could add data here if needed, e.g., the new UserInput string
}
