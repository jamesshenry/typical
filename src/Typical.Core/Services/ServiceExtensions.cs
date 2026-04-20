using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Text;
using Typical.Core.ViewModels;

namespace Typical.Core.Services;

public static class ServiceExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITextProvider, StaticTextProvider>();
        services.AddSingleton<GameOptions>(GameOptions.Default);
        services.AddSingleton<GameEngine>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<TypingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddSingleton<StatsViewModel>();
    }
}
