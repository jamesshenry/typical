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
