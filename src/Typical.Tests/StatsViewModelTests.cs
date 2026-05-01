using CommunityToolkit.Mvvm.Messaging;

using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;

namespace Typical.Tests;

public class StatsViewModelTests
{
    [Test]
    public async Task Receive_GamesStateUpdatedEvent_UpdatesViewModelCorrectly()
    {
        var messenger = WeakReferenceMessenger.Default;

        var sut = new StatsViewModel();

        var fakeState = new GameSnapshot(
            WordsPerMinute: 65.8,
            Accuracy: (Accuracy)98.5,
            Chars: new CharacterStats(0, 0, 0, 0),
            ElapsedTime: TimeSpan.FromSeconds(30),
            IsRunning: true,
            TargetText: "Test",
            UserInput: "Test",
            IsOver: true
        );

        var gameEvent = new GameStateUpdatedMessage(State: fakeState);

        messenger.Send(gameEvent);

        await Assert.That(sut.Stats.WordsPerMinute).IsEqualTo(65.8);
        await Assert.That(sut.Stats.Accuracy).IsEqualTo((Accuracy)98.5);
    }
}
