using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Typical.Core.Events;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public partial class StatsViewModel : ObservableObject, IRecipient<GameStateUpdatedMessage>
{
    [ObservableProperty]
    public partial GameStatisticsSnapshot? Stats { get; set; }

    public StatsViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(GameStateUpdatedMessage message)
    {
        Stats = message.Statistics;
    }
}
