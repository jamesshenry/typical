using Microsoft.Extensions.Logging;
using Typical.Core.Statistics;

namespace Typical.Core.Logging;

public static partial class CoreLogs
{
    // --- GameEngine Logs (2000-2099) ---
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "New game starting.")]
    public static partial void GameStarting(ILogger logger);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Game finished successfully."
    )]
    public static partial void GameFinished(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Game quit by user.")]
    public static partial void GameQuit(ILogger logger);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "Processing key: {KeyChar}, Type: {KeystrokeType}"
    )]
    public static partial void KeyProcessed(
        ILogger logger,
        char KeyChar,
        KeystrokeType KeystrokeType
    );

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Trace,
        Message = "Publishing game state update."
    )]
    public static partial void PublishingState(ILogger logger);

    // --- GameStats Logs (2100-2199) ---
    [LoggerMessage(EventId = 2100, Level = LogLevel.Debug, Message = "GameStats started.")]
    public static partial void StatsStarted(ILogger logger);

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message = "GameStats stopped. Elapsed: {ElapsedTime}ms"
    )]
    public static partial void StatsStopped(ILogger logger, double ElapsedTime);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Debug, Message = "GameStats reset.")]
    public static partial void StatsReset(ILogger logger);

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Debug,
        Message = "Key logged in stats: {Character} ({Type})"
    )]
    public static partial void StatsKeyLogged(ILogger logger, char Character, KeystrokeType Type);

    [LoggerMessage(EventId = 2104, Level = LogLevel.Debug, Message = "Backspace logged in stats.")]
    public static partial void StatsBackspaceLogged(ILogger logger);

    [LoggerMessage(
        EventId = 2105,
        Level = LogLevel.Trace,
        Message = "Recalculating all statistics."
    )]
    public static partial void RecalculatingStats(ILogger logger);
}
