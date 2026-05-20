using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Typical.Core.Events;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public partial class StatsViewModel : ObservableObject, IRecipient<GameStatsUpdatedMessage>
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    public partial GameStatsSnapshot Stats { get; set; } = GameStatsSnapshot.Empty;

    public StatsViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<StatsViewModel, GameStatsUpdatedMessage>(this, (r, m) => r.Receive(m));
    }

    public void Receive(GameStatsUpdatedMessage message)
    {
        Stats = message.State;
    }
}
