using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;

namespace Typical.Tests.Core.ViewModels;

public class StatsViewModelTests
{
    [Test]
    public async Task Receive_TestStatsUpdatedMessage_UpdatesProperties()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();
        var provider = services.BuildServiceProvider();
        var messenger = provider.GetRequiredService<IMessenger>();
        var sut = new StatsViewModel(messenger);

        var fakeSnapshot = new TestSnapshot(
            WPM: (Wpm)65.8,
            Accuracy: (Accuracy)98.5,
            Metrics: new TestMetrics(10, 1, 2),
            ElapsedTime: TimeSpan.FromSeconds(30)
        );
        var msg = new TestSessionUpdatedMessage(fakeSnapshot);

        messenger.Send(msg);

        await Assert.That(sut.Stats.WPM.Value).IsEqualTo(65.8);
        await Assert.That(sut.Stats.Accuracy.Value).IsEqualTo(98.5);
        await Assert.That(sut.Stats.Metrics.Correct).IsEqualTo(10);
        await Assert.That(sut.Stats.Metrics.Incorrect).IsEqualTo(1);
        await Assert.That(sut.Stats.Metrics.Corrections).IsEqualTo(2);
        await Assert.That(sut.Stats.ElapsedTime).IsEqualTo(TimeSpan.FromSeconds(30));
    }
}
