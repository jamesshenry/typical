using CommunityToolkit.Mvvm.Messaging;
using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Text;
using Typical.Core.ViewModels;

[assembly: GenerateImposter(typeof(IMessenger))]
[assembly: GenerateImposter(typeof(INavigationService))]

namespace Typical.Tests.Core.ViewModels;

public class TypingViewModelTests
{
    [Test]
    public async Task InitializeAsync_LoadsQuote_FromTextProvider()
    {
        var mockTextProvider = new MockTextProvider();
        mockTextProvider.SetText("Hello world!");
        var engine = new TypingSession(
            GameOptions.Default,
            NullLogger<TypingSession>.Instance,
            TimeProvider.System
        );
        var mockMessenger = IMessenger.Imposter();
        var mockNavigationService = INavigationService.Imposter();
        var vm = new TypingViewModel(
            engine,
            mockTextProvider,
            mockNavigationService.Instance(),
            NullLogger<TypingViewModel>.Instance,
            mockMessenger.Instance()
        );
        await vm.InitializeAsync();
        await Assert.That(vm.Target.Text).IsEqualTo("Hello world!");
    }

    [Test]
    public async Task ProcessInput_UpdatesEngine_AndBroadcastsMessage()
    {
        var mockTextProvider = new MockTextProvider();
        mockTextProvider.SetText("a");
        var engine = new TypingSession(
            GameOptions.Default,
            NullLogger<TypingSession>.Instance,
            TimeProvider.System
        );
        var mockMessenger = IMessenger.Imposter();

        var messenger = mockMessenger.Instance();
        var mockNavigationService = INavigationService.Imposter();
        var vm = new TypingViewModel(
            engine,
            mockTextProvider,
            mockNavigationService.Instance(),
            NullLogger<TypingViewModel>.Instance,
            mockMessenger.Instance()
        );
        await vm.InitializeAsync();
        vm.ProcessInput("a", false);
        // Assert observable state: engine should have processed the input
        await Assert.That(engine.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task Receive_GameResetMessage_ReloadsText_BasedOnSettings()
    {
        var mockTextProvider = new MockTextProvider();
        mockTextProvider.SetText("reset quote");
        var engine = new TypingSession(
            GameOptions.Default,
            NullLogger<TypingSession>.Instance,
            TimeProvider.System
        );
        var mockMessenger = IMessenger.Imposter();
        var mockNavigationService = INavigationService.Imposter();
        var vm = new TypingViewModel(
            engine,
            mockTextProvider,
            mockNavigationService.Instance(),
            NullLogger<TypingViewModel>.Instance,
            mockMessenger.Instance()
        );
        var msg = new GameResetMessage(new QuoteMode(QuoteLength.Short));
        vm.Receive(msg);
        await Task.Delay(10); // Allow async to complete
        await Assert.That(vm.Target.Text).IsEqualTo("reset quote");
    }
}
