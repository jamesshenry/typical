using Microsoft.Extensions.Logging;
using Typical;
using Typical.TUI;

public static partial class AppLogs
{
    // Define a log message with ID, level, template
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Application starting..."
    )]
    public static partial void ApplicationStarting(ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "No commands specified, starting interactive AppShell."
    )]
    public static partial void NoCommandsInteractive(ILogger logger);

    // Example with parameters
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Failed to process user {UserId}"
    )]
    public static partial void FailedToProcessUser(ILogger logger, int userId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Starting direct game with Mode: {Mode}, Duration: {Duration}"
    )]
    public static partial void StartingGame(ILogger logger, string mode, int duration);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = ("Application shutting down.")
    )]
    public static partial void ApplicationStopping(ILogger<AppShell> logger);
}
