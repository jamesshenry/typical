using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Text;
using Typical.Core.ViewModels;

namespace Typical.Core.Services;

public static class ServiceExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddMessenger();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITextProvider, TextProvider>();
        services.AddSingleton(TestOptions.Default);
        services.AddSingleton<TypingTest>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<TypingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddSingleton<StatsViewModel>();
        services.AddSingleton<ResultsViewModel>();
    }
}
