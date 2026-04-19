using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Text;
using Typical.Core.ViewModels;

namespace Typical.Core.Services;

public static class ServiceExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITextProvider, StaticTextProvider>(
            (_) =>
                new StaticTextProvider(
                    "You can cut down a tree with a hammer, but it takes about 30 days. If you trade the hammer for an ax, you can cut it down in about 30 minutes. The difference between 30 days and 30 minutes is skills."
                )
        );
        services.AddSingleton<GameOptions>(GameOptions.Default);
        services.AddSingleton<GameEngine>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<TypingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddSingleton<StatsViewModel>();
    }
}
