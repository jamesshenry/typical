using Microsoft.Extensions.Logging;
using Typical.Core.Statistics;

namespace Typical.Core.Logging;

public static partial class CoreLogs
{
    // --- TestEngine Logs (2000-2099) ---
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "New test starting.")]
    public static partial void TestStarting(ILogger logger);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Test finished successfully. {Stats}"
    )]
    public static partial void TestFinished(ILogger logger, TestSnapshot stats);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Test quit by user.")]
    public static partial void TestQuit(ILogger logger);

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
        Message = "Publishing test state update."
    )]
    public static partial void PublishingState(ILogger logger);

    // --- TestStats Logs (2100-2199) ---
    [LoggerMessage(EventId = 2100, Level = LogLevel.Debug, Message = "TestStats started.")]
    public static partial void StatsStarted(ILogger logger);

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message = "TestStats stopped. Elapsed: {ElapsedTime}ms"
    )]
    public static partial void StatsStopped(ILogger logger, double ElapsedTime);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Debug, Message = "TestStats reset.")]
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
