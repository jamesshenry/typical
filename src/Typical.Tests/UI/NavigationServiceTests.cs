using CommunityToolkit.Mvvm.Messaging;
using Imposter.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.ViewBase;
using Typical.Core;
using Typical.Core.Data;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Text;
using Typical.Core.ViewModels;
using Typical.Services;

[assembly: GenerateImposter(typeof(IDialogService))]
[assembly: GenerateImposter(typeof(ITextProvider))]

namespace Typical.Tests.UI;

public class NavigationServiceTests
{
    private readonly IDialogServiceImposter _dialogServiceMock = IDialogService.Imposter();
    private readonly ITextProviderImposter _textProviderMock = ITextProvider.Imposter();
    private readonly IStatsRepositoryImposter _statsRepoMock = IStatsRepository.Imposter();
    private ServiceProvider _serviceProvider = null!;
    private INavigationService _navigationService = null!;
    private IMessenger _messenger = null!;

    [Before(Test)]
    public void Setup()
    {
        var services = new ServiceCollection();

        _messenger = new StrongReferenceMessenger();
        services.AddSingleton(_messenger);

        services.AddSingleton(_dialogServiceMock.Instance());
        services.AddSingleton(_textProviderMock.Instance());
        services.AddSingleton(_statsRepoMock.Instance());

        services.AddSingleton(sp => new TypingTest(
            new GameOptions(),
            NullLogger<TypingTest>.Instance,
            TimeProvider.System
        ));

        services.AddSingleton<ILogger<HomeViewModel>>(NullLogger<HomeViewModel>.Instance);
        services.AddSingleton<ILogger<SettingsViewModel>>(NullLogger<SettingsViewModel>.Instance);
        services.AddSingleton<ILogger<TypingViewModel>>(NullLogger<TypingViewModel>.Instance);

        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<TypingViewModel>();

        services.AddTransient<Views.HomeView>();
        services.AddTransient<Views.SettingsView>();
        services.AddTransient<Views.TypingView>();

        services.AddSingleton<INavigationService>(sp => new NavigationService(
            sp,
            null!,
            sp.GetRequiredService<IMessenger>()
        ));

        _serviceProvider = services.BuildServiceProvider();

        _navigationService = (NavigationService)
            _serviceProvider.GetRequiredService<INavigationService>();
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
        _messenger.Register<NavigationChangedMessage>(this, (r, m) => receivedMessage = m);

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

    [Test]
    public async Task ViewLocator_MapsAllNavigatableViewModels()
    {
        var coreAssembly = typeof(HomeViewModel).Assembly;

        var navigatableTypes = coreAssembly
            .GetTypes()
            .Where(t => typeof(INavigatableView).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in navigatableTypes)
        {
            var viewModel = _serviceProvider.GetRequiredService(type);

            // Act
            var view = Navigation.ViewLocator.GetView(_serviceProvider, viewModel);

            // Assert
            await Assert.That(view).IsNotNull();
            await Assert.That(view).IsAssignableTo<View>();
        }
    }
}
