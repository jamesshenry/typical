using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Imposter.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.ViewModels;
using Typical.Services;

[assembly: GenerateImposter(typeof(GameEngine))]

namespace Typical.Tests;

public class NavigationServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private NavigationService _navigationService = null!;
    private IMessenger _messenger = null!;

    [Before(Test)]
    public void Setup()
    {
        var services = new ServiceCollection();
        _messenger = new StrongReferenceMessenger();

        // Register mock ViewModels
        services.AddTransient(sp => new HomeViewModel(
            _navigationService,
            NullLogger<HomeViewModel>.Instance
        ));
        var gameEngine = new GameEngine(GameOptions.Default, NullLogger<GameEngine>.Instance);
        services.AddTransient(sp => new TypingViewModel(
            gameEngine,
            new MockTextProvider(),
            _navigationService,
            NullLogger<TypingViewModel>.Instance
        ));

        services.AddSingleton<INavigationService>(sp => _navigationService);

        _serviceProvider = services.BuildServiceProvider();

        _navigationService = new NavigationService(_serviceProvider, null!, _messenger);
    }

    [After(Test)]
    public void CleanUp()
    {
        _messenger.UnregisterAll(this);
    }

    [Test]
    public async Task NavigateTo_ShouldBroadcastNavigationChangedMessage()
    {
        // Arrange
        NavigationChangedMessage? receivedMessage = null;
        _messenger.Register<NavigationChangedMessage>(
            this,
            (r, m) =>
            {
                receivedMessage = m;
            }
        );

        try
        {
            // Act
            _navigationService.NavigateTo<HomeViewModel>();

            // Assert
            await Assert.That(receivedMessage).IsNotNull();
            await Assert.That(receivedMessage!.Value).IsTypeOf<HomeViewModel>();
        }
        finally
        {
            _messenger.UnregisterAll(this);
        }
    }

    [Test]
    public async Task NavigateTo_ShouldUpdateCurrentViewModel()
    {
        // Act
        _navigationService.NavigateTo<TypingViewModel>();

        // Assert
        await Assert.That(_navigationService.CurrentViewModel).IsTypeOf<TypingViewModel>();
    }
}
