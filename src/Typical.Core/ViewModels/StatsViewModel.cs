using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Typical.Core.Events;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public partial class StatsViewModel : ObservableObject, IRecipient<StatisticsUpdatedMessage>
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    public partial TestSnapshot Stats { get; set; } = TestSnapshot.Empty;

    public StatsViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<StatsViewModel, StatisticsUpdatedMessage>(this, (r, m) => r.Receive(m));
    }

    public void Receive(StatisticsUpdatedMessage message)
    {
        Stats = message.State;
    }
}
