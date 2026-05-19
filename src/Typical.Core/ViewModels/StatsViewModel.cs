using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Typical.Core.Events;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public partial class StatsViewModel : ObservableObject, IRecipient<GameStatsUpdatedMessage>
{
    [ObservableProperty]
    public partial GameStatsSnapshot Stats { get; set; }

    public StatsViewModel()
    {
        WeakReferenceMessenger.Default.Register<StatsViewModel, GameStatsUpdatedMessage>(
            this,
            (r, m) => r.Receive(m)
        );
    }

    public void Receive(GameStatsUpdatedMessage message)
    {
        Stats = message.State;
    }
}
