# Codebase Analysis and Improvement Recommendations for Typical

## Overview

The Typical project is a console-based typing tutor application built with C# and Spectre.Console. It features a modular architecture with separate projects for core logic, TUI (Text User Interface), and tests. The application uses events for game state management, which is a good foundation for event-driven design.

## Current Event-Driven Architecture

The codebase has basic event support in the `GameEngine` class:

- `GameEnded` event: Fired when the game completes
- `StateChanged` event: Fired when user input changes

These events are consumed by `TypicalGame` for UI updates. This is a solid start, but the event system can be significantly expanded to improve maintainability, testability, and extensibility.

## Recommended Improvements

### 1. Expand Event-Driven Architecture

#### Additional Game Events

Introduce more granular events to decouple game logic from UI concerns:

```csharp
// In GameEngine.cs
public event EventHandler<KeyPressedEventArgs>? KeyPressed;
public event EventHandler<BackspacePressedEventArgs>? BackspacePressed;
public event EventHandler<GameStartedEventArgs>? GameStarted;
public event EventHandler<StatsUpdatedEventArgs>? StatsUpdated;
public event EventHandler<GamePausedEventArgs>? GamePaused;
public event EventHandler<GameResumedEventArgs>? GameResumed;

// Event args classes
public class KeyPressedEventArgs : EventArgs
{
    public char Character { get; }
    public KeystrokeType Type { get; }
    public int Position { get; }
}

public class StatsUpdatedEventArgs : EventArgs
{
    public GameStatisticsSnapshot Stats { get; }
}

public class GameStartedEventArgs : EventArgs
{
    public string TargetText { get; }
}
```

#### Event Aggregator Pattern

Implement an event aggregator to reduce coupling between components:

```csharp
public interface IEventAggregator
{
    void Publish<TEvent>(TEvent @event) where TEvent : class;
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
}

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    // Implementation...
}
```

#### Async Event Handling

Support async event handlers for better performance:

```csharp
public event Func<KeyPressedEventArgs, Task>? KeyPressedAsync;
```

### 2. UI State Management Improvements

#### Reactive UI Updates

Replace polling-based updates with event-driven rendering:

```csharp
// In TypicalGame.cs
private async Task HandleStateChanged(object? sender, GameStateChangedEventArgs e)
{
    await UpdateTypingAreaAsync();
    await UpdateStatsAreaAsync();
}

private async Task HandleStatsUpdated(object? sender, StatsUpdatedEventArgs e)
{
    await UpdateStatsAreaAsync();
}
```

#### UI Event Bus

Create a dedicated UI event system:

```csharp
public interface IUiEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}
```

### 3. Configuration and Settings Enhancements

#### Dynamic Configuration Reloading

Add support for runtime configuration changes:

```csharp
public interface IConfigurationService
{
    Task ReloadConfigurationAsync();
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}
```

#### Theme Switching Events

Make theme changes event-driven:

```csharp
// In ThemeManager.cs
public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

public async Task<bool> TrySetThemeAsync(string themeName)
{
    // ... existing logic ...
    if (success)
    {
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(themeName, _activeTheme));
    }
    return success;
}
```

### 4. Text Provider Enhancements

#### Text Loading Events

Add events for text provider lifecycle:

```csharp
public interface ITextProvider
{
    Task<string> GetTextAsync();
    event EventHandler<TextLoadingEventArgs>? TextLoading;
    event EventHandler<TextLoadedEventArgs>? TextLoaded;
    event EventHandler<TextLoadErrorEventArgs>? TextLoadError;
}
```

#### Multiple Text Sources

Support for different text sources with events:

```csharp
public class CompositeTextProvider : ITextProvider
{
    private readonly IEnumerable<ITextProvider> _providers;
    
    public async Task<string> GetTextAsync()
    {
        foreach (var provider in _providers)
        {
            try
            {
                var text = await provider.GetTextAsync();
                TextLoaded?.Invoke(this, new TextLoadedEventArgs(text, provider.GetType().Name));
                return text;
            }
            catch (Exception ex)
            {
                TextLoadError?.Invoke(this, new TextLoadErrorEventArgs(ex, provider.GetType().Name));
            }
        }
        throw new InvalidOperationException("No text providers available");
    }
    
    // Events...
}
```

### 5. Statistics and Analytics

#### Real-time Statistics Events

Enhance statistics with more granular events:

```csharp
// In GameStats.cs
public event EventHandler<WpmUpdatedEventArgs>? WpmUpdated;
public event EventHandler<AccuracyUpdatedEventArgs>? AccuracyUpdated;
public event EventHandler<KeystrokeLoggedEventArgs>? KeystrokeLogged;

private void OnStatsChanged()
{
    WpmUpdated?.Invoke(this, new WpmUpdatedEventArgs(_cachedWpm));
    AccuracyUpdated?.Invoke(this, new AccuracyUpdatedEventArgs(_cachedAccuracy));
    StatsUpdated?.Invoke(this, new StatsUpdatedEventArgs(CreateSnapshot()));
}
```

### 6. Error Handling and Logging

#### Global Error Events

Implement application-wide error handling:

```csharp
public interface IErrorHandler
{
    event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
    Task HandleErrorAsync(Exception exception, string context);
}

public class ErrorOccurredEventArgs : EventArgs
{
    public Exception Exception { get; }
    public string Context { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
```

### 7. Plugin Architecture

#### Event-Based Plugin System

Create a plugin system using events:

```csharp
public interface IPluginManager
{
    event EventHandler<PluginLoadedEventArgs>? PluginLoaded;
    event EventHandler<PluginUnloadedEventArgs>? PluginUnloaded;
    
    Task LoadPluginAsync(string pluginPath);
    Task UnloadPluginAsync(string pluginName);
}

public interface IPlugin
{
    string Name { get; }
    Task InitializeAsync(IEventAggregator eventAggregator);
    Task ShutdownAsync();
}
```

### 8. Testing Improvements

#### Event Testing Infrastructure

Create test helpers for event verification:

```csharp
public class EventRecorder<TEvent> where TEvent : EventArgs
{
    private readonly List<TEvent> _events = new();
    
    public void Record(object? sender, TEvent e) => _events.Add(e);
    
    public IReadOnlyList<TEvent> Events => _events;
    
    public void Clear() => _events.Clear();
}
```

### 9. Performance Optimizations

#### Event Debouncing

Implement debouncing for high-frequency events:

```csharp
public class DebouncedEvent<T> where T : EventArgs
{
    private readonly TimeSpan _delay;
    private readonly Action<T> _action;
    private CancellationTokenSource? _cts;
    
    public void Raise(T args)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        Task.Delay(_delay, _cts.Token).ContinueWith(_ => 
        {
            if (!_cts.IsCancellationRequested)
                _action(args);
        });
    }
}
```

### 10. Modern C# Features

#### Records and Pattern Matching

Use more modern C# features:

```csharp
// Enhanced event args using records
public record KeyPressedEventArgs(char Character, KeystrokeType Type, int Position, DateTime Timestamp);

// Pattern matching in event handlers
private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
{
    var action = e.Type switch
    {
        KeystrokeType.Correct => HandleCorrectKey(e),
        KeystrokeType.Incorrect => HandleIncorrectKey(e),
        KeystrokeType.Extra => HandleExtraKey(e),
        KeystrokeType.Correction => HandleCorrection(e),
        _ => throw new InvalidOperationException($"Unknown keystroke type: {e.Type}")
    };
    
    action();
}
```

## Implementation Priority

1. **High Priority**: Expand core game events (KeyPressed, StatsUpdated, GameStarted)
2. **Medium Priority**: Implement event aggregator and async event handling
3. **Low Priority**: Plugin system and advanced features

## Benefits of Enhanced Event-Driven Architecture

- **Maintainability**: Loose coupling between components
- **Testability**: Easier to test individual components in isolation
- **Extensibility**: New features can be added without modifying existing code
- **Performance**: Async event handling and debouncing
- **User Experience**: More responsive UI with real-time updates

## Migration Strategy

1. Start by adding new events alongside existing ones
2. Gradually refactor UI code to use new events
3. Implement event aggregator
4. Add async support
5. Create plugin architecture for future extensibility

This enhanced event-driven approach will make the codebase more maintainable, testable, and extensible while preserving the existing functionality.
