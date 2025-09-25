using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Typical.TUI;
using Typical.TUI.Views;

namespace Typical;

// The [Command] attribute on the class is optional but good practice.
public class ApplicationCommands
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationCommands> _logger;

    // The DI container will inject the services we need here.
    public ApplicationCommands(
        IServiceProvider serviceProvider,
        ILogger<ApplicationCommands> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// The entry point for interactive mode (when no arguments are given).
    /// </summary>
    [Command("")]
    public async Task RunInteractive()
    {
        AppLogs.NoCommandsInteractive(_logger);

        // Resolve the AppShell from the DI container and run it.
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        await appShell.RunAsync();
    }

    /// <summary>
    /// Directly starts a typing game, bypassing the main menu.
    /// </summary>
    [Command("play")]
    public async Task Play(string mode = "Quote", int duration = 60)
    {
        AppLogs.StartingGame(_logger, mode, duration);

        // Resolve a GameView directly from the DI container.
        // This is a "one-shot" game session.
        var gameView = _serviceProvider.GetRequiredService<GameView>();

        // We would need to pass these options to the GameView to configure the game.
        // For example: await gameView.RunAsync(new GameOptions { Mode = mode, Duration = duration });
        await gameView.RenderAsync(); // Simplified for this example
    }

    /// <summary>
    /// Displays user statistics directly.
    /// </summary>
    [Command("stats")]
    public async Task ShowStats()
    {
        _logger.LogInformation("Displaying stats view.");
        var statsView = _serviceProvider.GetRequiredService<StatsView>();
        await statsView.RenderAsync();
    }
}
