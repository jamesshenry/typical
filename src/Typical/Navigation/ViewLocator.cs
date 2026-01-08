using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui.ViewBase;
using Typical.Core.ViewModels;
using Typical.Views;

namespace Typical.Navigation;

public static class ViewLocator
{
    public static View GetView(IServiceProvider sp, object viewModel) =>
        viewModel switch
        {
            HomeViewModel => sp.GetRequiredService<HomeView>(),
            SettingsViewModel => sp.GetRequiredService<SettingsView>(),
            TypingViewModel => sp.GetRequiredService<TypingGameView>(),
            _ => throw new ArgumentException($"No view registered for {viewModel.GetType()}"),
        };
}
