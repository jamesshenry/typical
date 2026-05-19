using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;

namespace Typical.Tests.Core.ViewModels;

public class StatsViewModelTests
{
    [Test]
    public async Task Receive_GameStatsUpdatedMessage_UpdatesProperties()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();
        var provider = services.BuildServiceProvider();
        var messenger = provider.GetRequiredService<IMessenger>();
        var sut = new StatsViewModel();

        // Manually register with the test messenger
        messenger.Register<StatsViewModel, GameStatsUpdatedMessage>(sut, (r, m) => r.Receive(m));

        var fakeSnapshot = new GameStatsSnapshot(
            WPM: (WPM)65.8,
            Accuracy: (Accuracy)98.5,
            Chars: new CharacterStats(10, 1, 2),
            ElapsedTime: TimeSpan.FromSeconds(30)
        );
        var msg = new GameStatsUpdatedMessage(fakeSnapshot);

        messenger.Send(msg);

        await Assert.That(sut.Stats.WPM.Value).IsEqualTo(65.8);
        await Assert.That(sut.Stats.Accuracy.Value).IsEqualTo(98.5);
        await Assert.That(sut.Stats.Chars.Correct).IsEqualTo(10);
        await Assert.That(sut.Stats.Chars.Incorrect).IsEqualTo(1);
        await Assert.That(sut.Stats.Chars.Corrections).IsEqualTo(2);
        await Assert.That(sut.Stats.ElapsedTime).IsEqualTo(TimeSpan.FromSeconds(30));
    }
}
