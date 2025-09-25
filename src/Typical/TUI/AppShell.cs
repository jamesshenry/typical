using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Typical.TUI.Views;

namespace Typical.TUI;

public class AppShell
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppShell> _logger;

    public AppShell(IServiceProvider serviceProvider, ILogger<AppShell> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        AppLogs.ApplicationStarting(_logger);
        IView currentView = _serviceProvider.GetRequiredService<GameView>();

        if (currentView != null)
        {
            await currentView.RenderAsync();

            // The view's RenderAsync method would return the next view to transition to,
            // or null to quit.
            // e.g., MainMenuView returns a new GameView when the user selects "Start".
            // currentView = await currentView.GetNextViewAsync();
        }
        AppLogs.ApplicationStopping(_logger);
    }
}
