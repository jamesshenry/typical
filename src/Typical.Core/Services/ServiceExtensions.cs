using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Text;
using Typical.Core.ViewModels;

namespace Typical.Core.Services;

public static class ServiceExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        // Singleton: The provider and factory live for the app lifetime
        services.AddSingleton<ITextProvider, StaticTextProvider>();
        services.AddSingleton<IGameEngineFactory, GameEngineFactory>();

        services.AddSingleton<MainViewModel>();

        // Transient: A fresh ViewModel and Engine logic for every game session
        services.AddTransient<TypingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();

        // If you need the EventAggregator for UI-wide messages (like "New High Score")
        // keep it, but don't use it for character-by-character logic.
        // services.AddSingleton<IEventAggregator, EventAggregator>();
    }
}
