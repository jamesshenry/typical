# AGENT TASK: Enhance Logging Capabilities in `Typical.Core`

## CONTEXT

The `Typical.Core` project, which contains the application's core business logic, lacks a dedicated logging mechanism. The `Typical` TUI project has logging, but the two are not standardized. This task involves introducing structured, high-performance logging to `Typical.Core` and standardizing the logging `EventId` convention across the entire solution to improve debuggability.

## REQUIREMENTS

You must perform the following actions:

1. **Standardize `EventId` Convention:** Update all logging definitions to follow a layered numbering scheme.
    * **`Typical` (TUI Layer):** Use `EventId`s in the `1000` range.
    * **`Typical.Core` (Business Logic):** Use `EventId`s in the `2000` range.

2. **Create Logging Definitions for Core Logic:** Create a new static class `CoreLogs.cs` in `Typical.Core` to house all business logic-related logging definitions using the `[LoggerMessage]` source generator.

3. **Inject and Use Loggers in Core Classes:** Modify `GameEngine.cs` and `GameStats.cs` to accept `ILogger` via dependency injection and add calls to the new logging methods at key execution points.

4. **Enhance Serilog Configuration:** Update the Serilog configuration in `ServiceExtensions.cs` to separate log verbosity. The console sink should be restricted to `Information` level and higher, while the file sink should capture more detailed `Debug` level logs.

---

## FILE MODIFICATIONS

### 1. File to be Modified: `src/Typical/Logging/AppLogs.cs`

**Action:** Replace the entire contents of the file with the code below to standardize the `EventId`s.

```csharp
using Microsoft.Extensions.Logging;
using Typical;
using Typical.TUI;

public static partial class AppLogs
{
    // Define a log message with ID, level, template
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Application starting...")]
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
```

### 2. File to be Created: `src/Typical.Core/Logging/CoreLogs.cs`

**Action:** Create a new file at the specified path with the following content.

```csharp
using Microsoft.Extensions.Logging;
using Typical.Core.Statistics;

namespace Typical.Core.Logging;

public static partial class CoreLogs
{
    // --- GameEngine Logs (2000-2099) ---
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "New game starting.")]
    public static partial void GameStarting(ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Game finished successfully.")]
    public static partial void GameFinished(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Game quit by user.")]
    public static partial void GameQuit(ILogger logger);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Processing key: {KeyChar}, Type: {KeystrokeType}")]
    public static partial void KeyProcessed(ILogger logger, char KeyChar, KeystrokeType KeystrokeType);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Trace, Message = "Publishing game state update.")]
    public static partial void PublishingState(ILogger logger);

    // --- GameStats Logs (2100-2199) ---
    [LoggerMessage(EventId = 2100, Level = LogLevel.Debug, Message = "GameStats started.")]
    public static partial void StatsStarted(ILogger logger);

    [LoggerMessage(EventId = 2101, Level = LogLevel.Debug, Message = "GameStats stopped. Elapsed: {ElapsedTime}ms")]
    public static partial void StatsStopped(ILogger logger, double ElapsedTime);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Debug, Message = "GameStats reset.")]
    public static partial void StatsReset(ILogger logger);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Debug, Message = "Key logged in stats: {Character} ({Type})")]
    public static partial void StatsKeyLogged(ILogger logger, char Character, KeystrokeType Type);

    [LoggerMessage(EventId = 2104, Level = LogLevel.Debug, Message = "Backspace logged in stats.")]
    public static partial void StatsBackspaceLogged(ILogger logger);
    
    [LoggerMessage(EventId = 2105, Level = LogLevel.Trace, Message = "Recalculating all statistics.")]
    public static partial void RecalculatingStats(ILogger logger);
}
```

### 3. File to be Modified: `src/Typical.Core/GameEngine.cs`

**Action:** Replace the entire contents of the file with the code below to inject the logger and add logging calls.

```csharp
using System.Text;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngine
{
    private readonly StringBuilder _userInput;
    private readonly ITextProvider _textProvider;
    private readonly GameOptions _gameOptions;
    private readonly IEventAggregator _eventAggregator;
    private readonly GameStats _stats;
    private readonly ILogger<GameEngine> _logger;

    public GameEngine(ITextProvider textProvider, IEventAggregator eventAggregator, ILogger<GameEngine> logger, ILoggerFactory loggerFactory)
        : this(textProvider, eventAggregator, new GameOptions(), logger, loggerFactory) { }

    public GameEngine(
        ITextProvider textProvider,
        IEventAggregator eventAggregator,
        GameOptions gameOptions,
        ILogger<GameEngine> logger,
        ILoggerFactory loggerFactory
    )
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        _userInput = new StringBuilder();
        _eventAggregator = eventAggregator;
        _logger = logger;
        _stats = new GameStats(_eventAggregator, null, loggerFactory.CreateLogger<GameStats>());
    }

    public string TargetText { get; private set; } = string.Empty;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && _stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public bool ProcessKeyPress(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
        {
            IsOver = true;
            _stats.Stop();
            CoreLogs.GameQuit(_logger);
            _eventAggregator.Publish(new GameQuitEvent());
            return false;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (_userInput.Length > 0)
            {
                _userInput.Remove(_userInput.Length - 1, 1);
                _eventAggregator.Publish(new BackspacePressedEvent());
                PublishStateUpdate();
            }
            return true;
        }

        if (char.IsControl(key.KeyChar))
        {
            return true;
        }
        char inputChar = key.KeyChar;
        KeystrokeType type = DetermineKeystrokeType(inputChar);

        CoreLogs.KeyProcessed(_logger, inputChar, type);
        _eventAggregator.Publish(new KeyPressedEvent(inputChar, type, _userInput.Length));

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(key.KeyChar);
        }

        CheckEndCondition();
        PublishStateUpdate();

        return true;
    }

    private KeystrokeType DetermineKeystrokeType(char inputChar)
    {
        int currentPos = _userInput.Length;
        if (currentPos >= TargetText.Length)
            return KeystrokeType.Extra;
        if (inputChar == TargetText[currentPos])
            return KeystrokeType.Correct;
        return KeystrokeType.Incorrect;
    }

    private void CheckEndCondition()
    {
        if (_userInput.ToString() == TargetText)
        {
            IsOver = true;
            _stats.Stop();
            CoreLogs.GameFinished(_logger);
            _eventAggregator.Publish(new GameEndedEvent());
        }
    }

    public async Task StartNewGame()
    {
        CoreLogs.GameStarting(_logger);
        TargetText = await _textProvider.GetTextAsync();
        _stats.Start();
        _userInput.Clear();
        IsOver = false;
        PublishStateUpdate();
    }

    private void PublishStateUpdate()
    {
        CoreLogs.PublishingState(_logger);
        var snapShot = _stats.CreateSnapshot();
        var stateEvent = new GameStateUpdatedEvent(TargetText, UserInput, snapShot, IsOver);
        _eventAggregator.Publish(stateEvent);
    }
}
```

### 4. File to be Modified: `src/Typical.Core/Statistics/GameStats.cs`

**Action:** Replace the entire contents of the file with the code below to inject the logger and add logging calls.

```csharp
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;

namespace Typical.Core.Statistics;

internal class GameStats
{
    private readonly IEventAggregator _eventAggregator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GameStats> _logger;
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private long? _startTimestamp;
    private long? _endTimestamp;
    private bool _statsAreDirty = true;
    private double _cachedWpm;
    private double _cachedAccuracy;
    private CharacterStats _cachedChars = new(0, 0, 0, 0);

    public GameStats(IEventAggregator eventAggregator, TimeProvider? timeProvider, ILogger<GameStats> logger)
    {
        _eventAggregator = eventAggregator;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger;
        _eventAggregator.Subscribe<KeyPressedEvent>(OnKeyPressed);
        _eventAggregator.Subscribe<BackspacePressedEvent>(OnBackspacePressed);
    }

    private void OnBackspacePressed(BackspacePressedEvent @event)
    {
        if (!IsRunning) return;
        
        CoreLogs.StatsBackspaceLogged(_logger);
        _keystrokeHistory.RemoveLastCharacterLog();
        _keystrokeHistory.Add(new KeystrokeLog('\b', KeystrokeType.Correction, _timeProvider.GetTimestamp()));
        _statsAreDirty = true;
    }

    private void OnKeyPressed(KeyPressedEvent @event)
    {
        if (!IsRunning) Start();
        
        CoreLogs.StatsKeyLogged(_logger, @event.Character, @event.Type);
        _keystrokeHistory.Add(new KeystrokeLog(@event.Character, @event.Type, _timeProvider.GetTimestamp()));
        _statsAreDirty = true;
    }

    public double WordsPerMinute { get { if (_statsAreDirty) RecalculateAllStats(); return _cachedWpm; } }
    public double Accuracy { get { if (_statsAreDirty) RecalculateAllStats(); return _cachedAccuracy; } }
    public CharacterStats Chars { get { if (_statsAreDirty) RecalculateAllStats(); return _cachedChars; } }
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;
    public TimeSpan ElapsedTime => _timeProvider.GetElapsedTime(_startTimestamp ?? 0, _endTimestamp ?? _timeProvider.GetTimestamp());

    public void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
        CoreLogs.StatsStarted(_logger);
    }

    public void Reset()
    {
        _startTimestamp = null;
        _endTimestamp = null;
        _keystrokeHistory.Clear();
        _cachedWpm = 0;
        _cachedAccuracy = 100;
        _cachedChars = new CharacterStats(0, 0, 0, 0);
        CoreLogs.StatsReset(_logger);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
            CoreLogs.StatsStopped(_logger, ElapsedTime.TotalMilliseconds);
        }
    }

    public GameStatisticsSnapshot CreateSnapshot()
    {
        if (_statsAreDirty) RecalculateAllStats();
        return new GameStatisticsSnapshot(
            WordsPerMinute: _cachedWpm,
            Accuracy: _cachedAccuracy,
            Chars: _cachedChars,
            ElapsedTime: this.ElapsedTime,
            IsRunning: this.IsRunning
        );
    }

    private void RecalculateAllStats()
    {
        CoreLogs.RecalculatingStats(_logger);
        _cachedWpm = _keystrokeHistory.CalculateWpm(ElapsedTime);
        _cachedAccuracy = _keystrokeHistory.CalculateAccuracy();
        _cachedChars = _keystrokeHistory.GetCharacterStats();
        _statsAreDirty = false;
    }
}
```

### 5. File to be Modified: `src/Typical/Services/ServiceExtensions.cs`

**Action:** Replace the `ConfigureSerilog` method with the updated version below. Ensure the `using Serilog.Events;` directive is present at the top of the file.

```csharp
// Ensure this using directive exists at the top of the file
using Serilog.Events;

// ... inside the ServiceExtensions class ...

public static void ConfigureSerilog(this ILoggingBuilder builder)
{
    const string outputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceClass}) {Message:lj}{NewLine}{Exception}";
    builder.AddSerilog(
        new LoggerConfiguration()
            .MinimumLevel.Debug() // Set a default minimum level
            .WriteTo.File(
                formatter: new MessageTemplateTextFormatter(outputTemplate),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log"),
                shared: true,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug // Log debug events to file
            )
            .Enrich.WithProperty("ApplicationName", "<APP NAME>")
            .Enrich.With<SourceClassEnricher>()
            .WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Sixteen, restrictedToMinimumLevel: LogEventLevel.Information) // Keep console clean
            .CreateLogger()
    );
}
```
