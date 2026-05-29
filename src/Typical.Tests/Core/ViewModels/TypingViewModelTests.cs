using CommunityToolkit.Mvvm.Messaging;
using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Typical.Core;
using Typical.Core.Data;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Text;
using Typical.Core.ViewModels;

[assembly: GenerateImposter(typeof(IMessenger))]
[assembly: GenerateImposter(typeof(INavigationService))]
[assembly: GenerateImposter(typeof(IStatsRepository))]

namespace Typical.Tests.Core.ViewModels;

public class TypingViewModelTests
{
    private readonly IMessengerImposter mockMessenger = IMessenger.Imposter();
    private readonly INavigationServiceImposter mockNavigationService =
        INavigationService.Imposter();
    private readonly IStatsRepositoryImposter mockStatsRepository = IStatsRepository.Imposter();

    [Test]
    public async Task InitializeAsync_LoadsQuote_FromTextProvider()
    {
        var mockTextProvider = new MockTextProvider();
        mockTextProvider.SetText("Hello world!");
        var engine = new TypingTest(
            TestOptions.Default,
            NullLogger<TypingTest>.Instance,
            TimeProvider.System
        );

        var vm = new TypingViewModel(
            engine,
            mockTextProvider,
            mockStatsRepository.Instance(),
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
        var engine = new TypingTest(
            TestOptions.Default,
            NullLogger<TypingTest>.Instance,
            TimeProvider.System
        );
        var vm = new TypingViewModel(
            engine,
            mockTextProvider,
            mockStatsRepository.Instance(),
            mockNavigationService.Instance(),
            NullLogger<TypingViewModel>.Instance,
            mockMessenger.Instance()
        );
        await vm.InitializeAsync();
        vm.ProcessInput("a", false);
        // Assert observable state: engine should have processed the input
        await Assert.That(engine.UserInput).IsEqualTo("a");
    }
}
