# Codebase Analysis for Typical

## Project Overview

This is a console application similar to monkeytype, aimed at helping users improve their typing skills.

## Analysis Goals

Please analyze the following codebase for potential bugs, performance improvements, and adherence to modern C# best practices.
Specifically, there is currently a bug in the TypicalGame where backspaces arent registerd in the UI until the stats screen refreshes

## Directory Structure

Below is a summary of the directory structure to provide context for the file organization.
          - Typical
          - Typical.Core
          - Typical.Tests
            - TUI\
              - Enums
              - Runtime
              - Settings
            - Events
            - Statistics
            - Text
            - Core
            - TUI

## --- Start of Code Files ---

// File: src\Typical\TUI\Enums\HorizontalAlignment.cs

```cs
namespace Typical.TUI.Settings;

public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
}
```

// File: src\Typical\TUI\Enums\LayoutDirection.cs

```cs
namespace Typical.TUI.Settings;

public enum LayoutDirection
{
    Rows,
    Columns,
}
```

// File: src\Typical\TUI\Enums\VerticalAlignment.cs

```cs
namespace Typical.TUI.Settings;

public enum VerticalAlignment
{
    Top,
    Middle,
    Bottom,
}
```

// File: src\Typical\TUI\Runtime\LayoutConversion.cs

```cs
using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public static class LayoutConversion
{
    public static LayoutNode ToRuntimeRoot(this LayoutDefinition def, string? name = null)
    {
        return new LayoutNode(
            Ratio: def.Ratio ?? 1,
            Direction: ParseDirection(def.SplitDirection, name),
            Children: def.Children.ToDictionary(
                c => ValidateLayoutSectionName(c.Name),
                c => c.ToRuntimeNode()
            )
        );
    }

    private static LayoutNode ToRuntimeNode(this LayoutDefinition def)
    {
        return new LayoutNode(
            Ratio: def.Ratio ?? 1,
            Direction: ParseDirection(def.SplitDirection, def.Name),
            Children: def.Children.ToDictionary(
                c => ValidateLayoutSectionName(c.Name),
                c => c.ToRuntimeNode()
            )
        );
    }

    // Validates that a root layout name is allowed
    private static LayoutName ValidateRootLayoutName(string name)
    {
        var candidate = LayoutName.From(name);
        if (!LayoutName.All.Contains(candidate))
            throw new InvalidOperationException(
                $"Invalid root layout '{name}'. Allowed roots: {string.Join(", ", LayoutName.All)}"
            );
        return candidate;
    }

    private static LayoutSection ValidateLayoutSectionName(string name)
    {
        var candidate = LayoutSection.From(name);
        if (!LayoutSection.All.Contains(candidate))
        {
            throw new InvalidOperationException(
                $"Invalid layout name '{name}'. Allowed: {string.Join(", ", LayoutSection.All)}"
            );
        }

        return candidate;
    }

    private static LayoutDirection ParseDirection(string? raw, string? context) =>
        raw switch
        {
            "Rows" => LayoutDirection.Rows,
            "Columns" => LayoutDirection.Columns,
            _ => throw new InvalidOperationException(
                $"Invalid SplitDirection '{raw}' in layout '{context ?? "<child>"}'"
            ),
        };
}
```

// File: src\Typical\TUI\Runtime\LayoutFactory.cs

```cs
using Spectre.Console;
using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public class LayoutFactory
{
    private readonly RuntimeLayoutDict _presets;

    public LayoutFactory(RuntimeLayoutDict presets)
    {
        _presets = presets;
    }

    public Layout Build(LayoutName rootLayout)
    {
        if (!_presets.TryGetValue(rootLayout, out var rootDefinition))
        {
            return new Layout(rootLayout.Value);
        }

        return BuildLayoutFromDefinition(rootDefinition, rootLayout.Value);
    }

    private Layout BuildLayoutFromDefinition(LayoutNode node, string name)
    {
        // Use root or child name
        var layout = new Layout(name);

        layout.Ratio(node.Ratio);

        if (node.Children.Count == 0)
            return layout;

        var childLayouts = node
            .Children.Select(kvp => BuildLayoutFromDefinition(kvp.Value, kvp.Key.Value))
            .ToArray();

        if (node.Direction == LayoutDirection.Rows)
            layout.SplitRows(childLayouts);
        else
            layout.SplitColumns(childLayouts);

        return layout;
    }
}
```

// File: src\Typical\TUI\Runtime\LayoutNode.cs

```cs
namespace Typical.TUI.Settings;

public record LayoutNode(
    int Ratio,
    LayoutDirection Direction,
    Dictionary<LayoutSection, LayoutNode> Children
);
```

// File: src\Typical\TUI\Runtime\ThemeConversion.cs

```cs
using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public static class ThemeConversion
{
    /// <summary>
    /// Converts a string-keyed Theme dictionary into a strongly-typed ThemeSettings dictionary.
    /// Validates that all keys are defined LayoutName values.
    /// </summary>
    public static RuntimeTheme ToRuntimeTheme(this Theme theme)
    {
        var result = new RuntimeTheme();

        foreach (var kvp in theme)
        {
            var layoutName = ValidateLayoutSection(kvp.Key);
            result[layoutName] = kvp.Value;
        }

        return result;
    }

    /// <summary>
    /// Converts a ThemeDict (string-keyed themes) to a runtime dictionary keyed by theme name
    /// with ThemeSettings as values.
    /// </summary>
    public static Dictionary<string, RuntimeTheme> ToRuntimeThemes(this ThemeDict themeDict)
    {
        return themeDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToRuntimeTheme());
    }

    private static LayoutSection ValidateLayoutSection(string name)
    {
        var candidate = LayoutSection.From(name);
        if (!LayoutSection.All.Contains(candidate))
        {
            throw new InvalidOperationException(
                $"Invalid layout section '{name}' in Theme. Allowed values: {string.Join(", ", LayoutName.All)}"
            );
        }

        return candidate;
    }
}

public class RuntimeTheme : Dictionary<LayoutSection, ElementStyle> { }

public class RuntimeThemeDict : Dictionary<string, RuntimeTheme> { }

public class RuntimeLayoutDict : Dictionary<LayoutName, LayoutNode>
{
    public RuntimeLayoutDict()
        : base() { }

    public RuntimeLayoutDict(Dictionary<LayoutName, LayoutNode> dictionary)
        : base(dictionary) { }
}
```

// File: src\Typical\TUI\Settings\AlignmentSettings.cs

```cs
namespace Typical.TUI.Settings;

public class AlignmentSettings
{
    public VerticalAlignment Vertical { get; set; }
    public HorizontalAlignment Horizontal { get; set; }
}
```

// File: src\Typical\TUI\Settings\AppSettings.cs

```cs
namespace Typical.TUI.Settings;

public class AppSettings
{
    public ThemeDict Themes { get; set; } = [];
    public LayoutPresetDict Layouts { get; set; } = [];
}
```

// File: src\Typical\TUI\Settings\AppSettingsExtensions.cs

```cs
using Typical.TUI.Runtime;

namespace Typical.TUI.Settings;

public static class AppSettingsExtensions
{
    public static RuntimeLayoutDict ToRuntimeLayouts(this LayoutPresetDict layoutDict)
    {
        return new RuntimeLayoutDict(
            layoutDict.ToDictionary(
                kvp => LayoutName.From(kvp.Key),
                kvp => kvp.Value.ToRuntimeRoot()
            )
        );
    }

    // Themes can stay string-keyed or convert similarly if needed
}
```

// File: src\Typical\TUI\Settings\BorderStyleSettings.cs

```cs
namespace Typical.TUI.Settings;

public class BorderStyleSettings
{
    public string? ForegroundColor { get; set; }
    public string? Decoration { get; set; }
}
```

// File: src\Typical\TUI\Settings\ElementStyle.cs

```cs
namespace Typical.TUI.Settings;

public class ElementStyle
{
    public BorderStyleSettings? BorderStyle { get; set; }
    public PanelHeaderSettings? PanelHeader { get; set; }
    public AlignmentSettings? Alignment { get; set; }
    public bool WrapInPanel { get; internal set; } = true;
}
```

// File: src\Typical\TUI\Settings\LayoutDefinition.cs

```cs
namespace Typical.TUI.Settings;

public class LayoutDefinition
{
    public string Name { get; set; } = default!;
    public int? Ratio { get; set; } = 1;
    public string? SplitDirection { get; set; } = "Columns";
    public List<LayoutDefinition> Children { get; set; } = [];
}
```

// File: src\Typical\TUI\Settings\LayoutName.cs

```cs
using System.Diagnostics.CodeAnalysis;
using Vogen;

namespace Typical.TUI.Settings;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial record LayoutName
{
    public static readonly LayoutName Default = From(nameof(Default));
    public static readonly LayoutName Dashboard = From(nameof(Dashboard));

    public static readonly IReadOnlySet<LayoutName> All = new HashSet<LayoutName>
    {
        Default,
        Dashboard,
    };
}
```

// File: src\Typical\TUI\Settings\LayoutSection.cs

```cs
using Vogen;

namespace Typical.TUI.Settings;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial record LayoutSection
{
    public static readonly LayoutSection Default = From(nameof(Default));
    public static readonly LayoutSection Header = From(nameof(Header));
    public static readonly LayoutSection Breadcrumb = From(nameof(Breadcrumb));
    public static readonly LayoutSection TypingArea = From(nameof(TypingArea));
    public static readonly LayoutSection Footer = From(nameof(Footer));
    public static readonly LayoutSection GeneralInfo = From(nameof(GeneralInfo));
    public static readonly LayoutSection GameInfo = From(nameof(GameInfo));
    public static readonly LayoutSection TypingInfo = From(nameof(TypingInfo));
    public static readonly LayoutSection Center = From(nameof(Center));

    public static readonly IReadOnlySet<LayoutSection> All = new HashSet<LayoutSection>
    {
        Default,
        Header,
        TypingArea,
        Footer,
        Breadcrumb,
        GameInfo,
        TypingInfo,
        Center,
    };
}
```

// File: src\Typical\TUI\Settings\PanelHeaderSettings.cs

```cs
namespace Typical.TUI.Settings;

public class PanelHeaderSettings
{
    public string? Text { get; set; }
}
```

// File: src\Typical\TUI\Settings\ThemeManager.cs

```cs
using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.TUI.Runtime;

namespace Typical.TUI.Settings;

public class ThemeManager
{
    private RuntimeTheme _activeTheme;
    private readonly Dictionary<string, RuntimeTheme>_themes;

    public string ActiveThemeName { get; private set; }

    public ThemeManager(Dictionary<string, RuntimeTheme> themes, string? defaultTheme = null)
    {
        if (themes?.Any() != true)
            throw new ArgumentException("No themes provided.");

        _themes = themes;

        ActiveThemeName = defaultTheme ?? _themes.Keys.First();
        _activeTheme = _themes[ActiveThemeName];
    }

    public IRenderable Apply<T>(T renderable, LayoutSection layoutName)
        where T : IRenderable
    {
        if (!_activeTheme.TryGetValue(layoutName, out var style))
        {
            _activeTheme.TryGetValue(LayoutSection.Default, out style);
        }

        style ??= new ElementStyle();

        if (layoutName == LayoutSection.Header)
            style.WrapInPanel = false;

        if (!style.WrapInPanel)
            return renderable;

        Panel finalPanel;

        if (renderable is Panel existingPanel)
        {
            finalPanel = existingPanel;
        }
        else
        {
            IRenderable content = renderable;
            if (style.Alignment is not null)
            {
                var verticalAlign = Enum.Parse<Spectre.Console.VerticalAlignment>(
                    style.Alignment.Vertical.ToString(),
                    true
                );

                content = Enum.Parse<Justify>(style.Alignment.Horizontal.ToString(), true) switch
                {
                    Justify.Left => Align.Left(content, verticalAlign),
                    Justify.Center => Align.Center(content, verticalAlign),
                    Justify.Right => Align.Right(content, verticalAlign),
                    _ => renderable,
                };
            }
            finalPanel = new Panel(content);
        }

        if (style.BorderStyle is not null)
        {
            var foreground = style.BorderStyle.ForegroundColor is not null
                ? ParseColor(style.BorderStyle.ForegroundColor)
                : Color.Default;

            Enum.TryParse<Decoration>(style.BorderStyle.Decoration, true, out var decoration);

            finalPanel.BorderStyle = new Style(foreground: foreground, decoration: decoration);
        }

        if (style.PanelHeader?.Text is not null)
        {
            finalPanel.Header = new PanelHeader(style.PanelHeader.Text);
        }
        return finalPanel.Expand();
    }

    private static Color? ParseColor(string stringColor)
    {
        if (stringColor.StartsWith('#'))
            return Color.FromHex(stringColor);

        return Enum.TryParse<ConsoleColor>(stringColor, out var consoleColor)
            ? (Color?)consoleColor
            : null;
    }

    public bool TrySetTheme(string themeName)
    {
        bool exists = _themes.TryGetValue(themeName, out var theme);
        if (exists && theme is not null)
        {
            _activeTheme = theme;
        }

        return exists && theme is not null;
    }
}
```

// File: src\Typical\TUI\Settings\ThemeSettings.cs

```cs
namespace Typical.TUI.Settings;

public class Theme : Dictionary<string, ElementStyle> { }

public class LayoutPresetDict : Dictionary<string, LayoutDefinition>;

public class ThemeDict : Dictionary<string, Theme> { }
```

// File: src\Typical\ConfigurationExtensions.cs

```cs
// using Microsoft.Extensions.Configuration;
// using Typical.TUI;
// using Typical.TUI.Settings;

// namespace Typical;

// public static class ConfigurationExtensions
// {
//     public static RuntimeTheme GetThemeSettings(this IConfiguration configuration)
//     {
//         var section = configuration.GetSection("Theme");
//         var dict = new RuntimeTheme();

//         foreach (var child in section.GetChildren())
//         {
//             var key = LayoutName.From(child.Key);
//             var value = child.Get<ElementStyle>();
//             dict[key] = value;
//         }

//         return dict;
//     }

//     // TODO:Not working
//     public static LayoutPresetDict GetLayoutPresets(this IConfiguration configuration)
//     {
//         var section = configuration.GetSection("Layouts");
//         var dict = new LayoutPresetDict();

//         foreach (var child in section.GetChildren())
//         {
//             var key = child.Key;
//             var value = child.Get<LayoutDefinition>();
//             dict[key] = value;
//         }

//         return dict;
//     }
// }
```

// File: src\Typical\MarkupGenerator.cs

```cs
using System.Text;
using Spectre.Console;

namespace Typical;

public class MarkupGenerator
{
    public Markup BuildMarkupOptimized(string target, string typed)
    {
        return new Markup(BuildMarkupString(target, typed));
    }

    internal string BuildMarkupString(string target, string typed)
    {
        if (string.IsNullOrEmpty(target))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var typedLength = typed.Length;
        TypingResult currentState = TypingResult.Untyped;

        if (typedLength > 0 && target.Length > 0)
        {
            currentState = target[0] == typed[0] ? TypingResult.Correct : TypingResult.Incorrect;
        }
        builder.Append(GetMarkupForState(currentState));

        for (int i = 0; i < target.Length; i++)
        {
            TypingResult charState;
            if (i >= typedLength)
            {
                charState = TypingResult.Untyped;
            }
            else
            {
                charState = target[i] == typed[i] ? TypingResult.Correct : TypingResult.Incorrect;
            }

            if (charState != currentState)
            {
                builder.Append("[/]");
                builder.Append(GetMarkupForState(charState));
                currentState = charState;
            }
            var escapedChar = Markup.Escape(target[i].ToString());

            if (i == typedLength)
            {
                builder.Append($"[underline]{escapedChar}[/]");
            }
            else
            {
                builder.Append(escapedChar);
            }
        }

        builder.Append("[/]");

        if (typedLength > target.Length)
        {
            builder.Append(GetMarkupForState(TypingResult.Incorrect));
            builder.Append($"{Markup.Escape(typed.Substring(target.Length))}");
            builder.Append("[/]");
        }

        return builder.ToString();
    }

    private string GetMarkupForState(TypingResult state) =>
        state switch
        {
            TypingResult.Correct => "[default on green]",
            TypingResult.Incorrect => "[red on grey15]",
            _ => "[grey]",
        };
}
```

// File: src\Typical\Program.cs

```cs
using System.Reflection;
using DotNetPathUtils;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Typical;
using Typical.Core;
using Typical.Core.Text;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;
using Velopack;

var pathHelper = new PathEnvironmentHelper(
    new PathUtilsOptions()
    {
        DirectoryNameCase = DirectoryNameCase.CamelCase,
        PrefixWithPeriod = false,
    }
);
if (OperatingSystem.IsWindows())
{
    var appDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
    VelopackApp
        .Build()
        .OnAfterInstallFastCallback(v => pathHelper.EnsureDirectoryIsInPath(appDirectory!))
        .OnBeforeUninstallFastCallback(v => pathHelper.RemoveDirectoryFromPath(appDirectory!))
        .Run();
}
var configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();

var appSettings = configuration.Get<AppSettings>()!;

var themeManager = new ThemeManager(appSettings.Themes.ToRuntimeThemes(), defaultTheme: "Default");
var layoutFactory = new LayoutFactory(appSettings.Layouts.ToRuntimeLayouts());

string quotePath = Path.Combine(AppContext.BaseDirectory, "quote.txt");

string text = File.Exists(quotePath)
    ? await File.ReadAllTextAsync(quotePath)
    : "The quick brown fox jumps over the lazy dog.";

ITextProvider textProvider = new StaticTextProvider(text);

var game = new GameEngine(textProvider);
await game.StartNewGame();
var markupGenerator = new MarkupGenerator();
var runner = new TypicalGame(
    game,
    themeManager,
    markupGenerator,
    layoutFactory,
    AnsiConsole.Console
);
runner.Run();
Console.Clear();
```

// File: src\Typical\StaticTextProvider.cs

```cs
using Typical.Core.Text;

namespace Typical;

internal class StaticTextProvider(string text) : ITextProvider
{
    private readonly string _text = text;

    public Task<string> GetTextAsync() => Task.FromResult(_text);
}
```

// File: src\Typical\TypicalGame.cs

```cs
using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
using Typical.Core.Events;
using Typical.TUI;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;

namespace Typical;

public class TypicalGame
{
    private readonly MarkupGenerator _markupGenerator;
    private readonly GameEngine_engine;
    private readonly ThemeManager _theme;
    private readonly LayoutFactory_layoutFactory;
    private readonly IAnsiConsole _console;
    private bool_needsTypingRefresh;
    private bool _needsStatsRefresh;

    public TypicalGame(
        GameEngine engine,
        ThemeManager theme,
        MarkupGenerator markupGenerator,
        LayoutFactory layoutFactory,
        IAnsiConsole console
    )
    {
        _engine = engine;
        _engine.GameEnded += OnEngineGameEnded;
        _engine.StateChanged += StateChanged;
        _theme = theme;
        _markupGenerator = markupGenerator;
        _layoutFactory = layoutFactory;
        _console = console;
    }

    private void StateChanged(object? sender, GameStateChangedEventArgs e)
    {
        _needsTypingRefresh = true;
    }

    private void OnEngineGameEnded(object? sender, GameEndedEventArgs e)
    {
        throw new NotImplementedException();
    }

    public void Run()
    {
        var layout = _layoutFactory.Build(LayoutName.Dashboard);
        const int statsUpdateIntervalMs = 1000; // Update stats every 2 seconds
        var statsTimer = Stopwatch.StartNew();
        _console
            .Live(layout)
            .Start(ctx =>
            {
                var typingArea = layout[LayoutSection.TypingArea.Value];
                var statsArea = layout[LayoutSection.GameInfo.Value];
                var headerArea = layout[LayoutSection.Header.Value];
                typingArea.Update(CreateTypingArea());
                statsArea.Update(CreateGameInfoArea());
                headerArea.Update(CreateHeader());

                ctx.Refresh();

                int lastHeight = Console.WindowHeight;
                int lastWidth = Console.WindowWidth;

                while (true)
                {
                    if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
                    {
                        lastWidth = Console.WindowWidth;
                        lastHeight = Console.WindowHeight;
                        _needsTypingRefresh = true;
                        _needsStatsRefresh = true;
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (!_engine.ProcessKeyPress(key))
                            break;
                    }

                    if (_engine.IsRunning && statsTimer.ElapsedMilliseconds > statsUpdateIntervalMs)
                    {
                        _needsStatsRefresh = true;
                        statsTimer.Restart(); // Reset the timer
                    }

                    if (_needsTypingRefresh || _needsStatsRefresh)
                    {
                        typingArea.Update(CreateTypingArea());
                        statsArea.Update(CreateGameInfoArea());
                        ctx.Refresh();
                        _needsTypingRefresh = false;
                        _needsStatsRefresh = false;
                    }

                    if (_engine.IsOver)
                    {
                        ctx.Refresh();
                        Thread.Sleep(500);
                        break;
                    }

                    Thread.Sleep(_engine.TargetFrameDelayMilliseconds);
                }
            });

        DisplaySummary();
    }

    private IRenderable CreateGameInfoArea()
    {
        var stats = _engine.GetGameStatistics();
        var grid = new Grid();
        grid.AddColumns([new GridColumn(), new GridColumn()]);
        grid.AddRow("WPM:", $"{stats.WordsPerMinute:F1}");
        grid.AddRow("Accuracy:", $"{stats.Accuracy:F1}%");
        grid.AddRow("Correct Chars:", $"{stats.Chars.Correct}");
        grid.AddRow("Incorrect Chars:", $"{stats.Chars.Incorrect}");
        grid.AddRow("Extra Chars:", $"{stats.Chars.Extra}");
        grid.AddRow("Elapsed:", $"{stats.ElapsedTime:mm\\:ss}");
        return _theme.Apply(grid, LayoutSection.GameInfo);
    }

    private IRenderable CreateTypingArea()
    {
        return new Panel(new Text(_engine.UserInput));
        // var markup = _markupGenerator.BuildMarkupOptimized(_engine.TargetText, _engine.UserInput);
        // return _theme.Apply(markup, LayoutSection.TypingArea);
    }

    private IRenderable CreateHeader()
    {
        return _theme.Apply(new Markup("Typical - A Typing Tutor"), LayoutSection.Header);
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
```

// File: src\Typical\TypingResult.cs

```cs
namespace Typical;

internal enum TypingResult
{
    Untyped,
    Correct,
    Incorrect,
}
```

// File: src\Typical.Core\Events\GameEndedEventArgs.cs

```cs
using Typical.Core.Statistics;

namespace Typical.Core.Events;

public class GameEndedEventArgs : EventArgs
{
    public GameEndedEventArgs(GameStatisticsSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public GameStatisticsSnapshot Snapshot { get; }
}

public class GameStateChangedEventArgs : EventArgs
{
    // You could add data here if needed, e.g., the new UserInput string
}
```

// File: src\Typical.Core\Events\GameStateChangedEventArgs.cs

```cs```
// File: src\Typical.Core\Statistics\CharacterStats.cs

```cs
namespace Typical.Core.Statistics;

// A simple record to hold the results of GetCharacterStats
public record CharacterStats(int Correct, int Incorrect, int Extra, int Corrections);
```

// File: src\Typical.Core\Statistics\GameStatisticsSnapshot.cs

```cs
namespace Typical.Core.Statistics;

public record GameStatisticsSnapshot(
    double WordsPerMinute,
    double Accuracy,
    CharacterStats Chars,
    TimeSpan ElapsedTime,
    bool IsRunning
);
```

// File: src\Typical.Core\Statistics\GameStats.cs

```cs
namespace Typical.Core.Statistics;

internal class GameStats(TimeProvider? timeProvider = null)
{
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private readonly TimeProvider_timeProvider = timeProvider ?? TimeProvider.System;
    private long? _startTimestamp;
    private long?_endTimestamp;
    private bool _statsAreDirty = true; // Start dirty
    private double_cachedWpm;
    private double _cachedAccuracy;
    private CharacterStats_cachedChars = new(0, 0, 0, 0);
    public double WordsPerMinute
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return_cachedWpm;
        }
    }

    public double Accuracy
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return _cachedAccuracy;
        }
    }

    public CharacterStats Chars
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return _cachedChars;
        }
    }
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;
    public TimeSpan ElapsedTime =>
        _timeProvider.GetElapsedTime(
            _startTimestamp ?? 0,
            _endTimestamp ?? _timeProvider.GetTimestamp()
        );

    public void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
    }

    public void Reset()
    {
        _startTimestamp = null;
        _endTimestamp = null;
        _keystrokeHistory.Clear();
        _cachedWpm = 0;
        _cachedAccuracy = 100;
        _cachedChars = new CharacterStats(0, 0, 0, 0);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
        }
    }

    public GameStatisticsSnapshot CreateSnapshot()
    {
        if (_statsAreDirty)
        {
            RecalculateAllStats();
        }

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
        _cachedWpm = _keystrokeHistory.CalculateWpm(ElapsedTime);
        _cachedAccuracy = _keystrokeHistory.CalculateAccuracy();
        _cachedChars = _keystrokeHistory.GetCharacterStats();

        _statsAreDirty = false;
    }

    internal void LogKeystroke(char keyChar, KeystrokeType extra)
    {
        if (!IsRunning)
        {
            Start();
        }
        _keystrokeHistory.Add(new KeystrokeLog(keyChar, extra, _timeProvider.GetTimestamp()));
        _statsAreDirty = true;
    }

    internal void LogCorrection()
    {
        _keystrokeHistory.RemoveLastCharacterLog();

        _keystrokeHistory.Add(
            new KeystrokeLog('\b', KeystrokeType.Correction, _timeProvider.GetTimestamp())
        );

        _statsAreDirty = true;
    }
}
```

// File: src\Typical.Core\Statistics\KeystrokeHistory.cs

```cs
using System.Collections;

namespace Typical.Core.Statistics;

public class KeystrokeHistory : IEnumerable<KeystrokeLog>
{
    private readonly List<KeystrokeLog> _logs = new();

    public int Count => _logs.Count;
    public int CorrectCount => _logs.Count(log => log.Type == KeystrokeType.Correct);
    public int IncorrectCount => _logs.Count(log => log.Type == KeystrokeType.Incorrect);
    public int ExtraCount => _logs.Count(log => log.Type == KeystrokeType.Extra);

    private (int Correct, int Incorrect, int Extra, int Corrections) GetCounts()
    {
        int correct = 0;
        int incorrect = 0;
        int extra = 0;
        int corrections = 0;

        foreach (var log in _logs)
        {
            switch (log.Type)
            {
                case KeystrokeType.Correct:
                    correct++;
                    break;
                case KeystrokeType.Incorrect:
                    incorrect++;
                    break;
                case KeystrokeType.Extra:
                    extra++;
                    break;
                case KeystrokeType.Correction:
                    corrections++;
                    break;
            }
        }
        return (correct, incorrect, extra, corrections);
    }

    public void Add(KeystrokeLog log)
    {
        _logs.Add(log);
    }

    public void Clear()
    {
        _logs.Clear();
    }

    public double CalculateWpm(TimeSpan duration) =>
        duration.TotalMinutes == 0
            ? 0
            : _logs.Count(log => log.Type == KeystrokeType.Correct) / 5.0 / duration.TotalMinutes;

    public double CalculateAccuracy()
    {
        if (Count == 0)
            return 100.0;

        var (correct, incorrect, _, _) = GetCounts();
        int totalChars = correct + incorrect;
        return totalChars == 0 ? 100.0 : (double)correct / totalChars * 100.0;
    }

    public CharacterStats GetCharacterStats()
    {
        var counts = GetCounts();
        return new CharacterStats(
            Correct: counts.Correct,
            Incorrect: counts.Incorrect,
            Extra: counts.Extra,
            Corrections: counts.Corrections
        );
    }

    public IEnumerator<KeystrokeLog> GetEnumerator() => _logs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void RemoveLastCharacterLog()
    {
        // Use FindLastIndex to search from the end of the list.
        int indexToRemove = _logs.FindLastIndex(log =>
            log.Type == KeystrokeType.Correct
            || log.Type == KeystrokeType.Incorrect
            || log.Type == KeystrokeType.Extra
        );

        // If a log was found (index is not -1), remove it.
        if (indexToRemove != -1)
        {
            _logs.RemoveAt(indexToRemove);
        }
    }
}
```

// File: src\Typical.Core\Statistics\KeystrokeLog.cs

```cs
namespace Typical.Core.Statistics;

public record struct KeystrokeLog(char Character, KeystrokeType Type, long Timestamp);
```

// File: src\Typical.Core\Statistics\KeystrokeType.cs

```cs
namespace Typical.Core.Statistics;

public enum KeystrokeType
{
    Correct,
    Incorrect,
    Extra,
    Correction,
}
```

// File: src\Typical.Core\Text\ITextProvider.cs

```cs
namespace Typical.Core.Text;

public interface ITextProvider
{
    Task<string> GetTextAsync();
}
```

// File: src\Typical.Core\GameEngine.cs

```cs
using System.Text;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngine
{
    private readonly StringBuilder _userInput;
    private readonly ITextProvider_textProvider;
    private readonly GameOptions _gameOptions;
    private readonly GameStats_stats;

    public GameEngine(ITextProvider textProvider)
        : this(textProvider, new GameOptions()) { }

    public GameEngine(ITextProvider textProvider, GameOptions gameOptions)
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        _userInput = new StringBuilder();
        _stats = new GameStats();
    }

    public string TargetText { get; private set; } = string.Empty;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && _stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public event EventHandler<GameEndedEventArgs>? GameEnded;
    public event EventHandler<GameStateChangedEventArgs>? StateChanged;

    public bool ProcessKeyPress(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
        {
            IsOver = true;
            _stats.Stop();
            return false;
        }

        if (key.Key == ConsoleKey.Backspace && _userInput.Length > 0)
        {
            _userInput.Remove(_userInput.Length - 1, 1);
            // _stats.LogCorrection(); // Assuming you have/want this method
            return true;
        }
        if (char.IsControl(key.KeyChar))
        {
            return true; // Ignore other control characters but continue the game
        }
        char inputChar = key.KeyChar;

        KeystrokeType type = DetermineKeystrokeType(inputChar);

        _stats.LogKeystroke(inputChar, type);

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(key.KeyChar);
            StateChanged?.Invoke(this, new GameStateChangedEventArgs());
        }

        CheckEndCondition();

        return true;
    }

    private KeystrokeType DetermineKeystrokeType(char inputChar)
    {
        int currentPos = _userInput.Length;

        if (currentPos >= TargetText.Length)
        {
            return KeystrokeType.Extra;
        }

        if (inputChar == TargetText[currentPos])
        {
            return KeystrokeType.Correct;
        }

        return KeystrokeType.Incorrect;
    }

    private void CheckEndCondition()
    {
        if (_userInput.ToString() == TargetText)
        {
            IsOver = true;
            _stats.Stop();

            GameEnded?.Invoke(this, new GameEndedEventArgs(_stats.CreateSnapshot()));
        }
    }

    public async Task StartNewGame()
    {
        TargetText = await _textProvider.GetTextAsync();
        _stats.Start();
        _userInput.Clear();
        IsOver = false;
    }

    public GameStatisticsSnapshot GetGameStatistics()
    {
        return _stats.CreateSnapshot();
    }
}
```

// File: src\Typical.Core\GameOptions.cs

```cs
namespace Typical.Core;

public record GameOptions
{
    public static GameOptions Default { get; set; } = new();
    public bool ForbidIncorrectEntries { get; set; } = false;
    public int TargetFrameRate { get; set; } = 60;
    // Future options could be added here:
    // public int TimeLimitSeconds { get; set; } = 0; // 0 for no limit
    // public bool ShowLiveWpm { get; set; } = false;
}
```

// File: src\Typical.Tests\Core\GameStatsTests.cs

```cs
using System;
using Microsoft.Extensions.Time.Testing;
using TUnit;
using Typical.Core.Statistics;

namespace Typical.Tests
{
    public class GameStatsTests
    {
        [Test]
        public async Task InitialState_ShouldBeDefaults()
        {
            var stats = new GameStats();

            await Assert.That(stats.WordsPerMinute).IsEqualTo(0);
            await Assert.That(stats.Accuracy).IsEqualTo(100);
            await Assert.That(stats.IsRunning).IsFalse();
        }

        [Test]
        public async Task Start_ShouldSetIsRunningTrue()
        {
            var fakeTime = new FakeTimeProvider();
            var stats = new GameStats(fakeTime);

            stats.Start();

            await Assert.That(stats.IsRunning).IsTrue();
        }

        [Test]
        public async Task Stop_ShouldSetIsRunningFalse()
        {
            var fakeTime = new FakeTimeProvider();
            var stats = new GameStats(fakeTime);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            stats.Stop();

            await Assert.That(stats.IsRunning).IsFalse();
        }

        [Test]
        public async Task Update_ShouldCalculateAccuracy()
        {
            var fakeTime = new FakeTimeProvider();
            var stats = new GameStats(fakeTime);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            string target = "hello";
            string input = "hxllo"; // 1 incorrect out of 5

            foreach (var (c, i) in target.Zip(input))
            {
                if (c == i)
                {
                    stats.LogKeystroke(c, KeystrokeType.Correct);
                }
                else
                {
                    stats.LogKeystroke(i, KeystrokeType.Incorrect);
                }
            }
            await Assert.That(stats.Accuracy).IsEqualTo(80);
        }

        [Test]
        public async Task Update_ShouldCalculateWordsPerMinute()
        {
            var fakeTime = new FakeTimeProvider();
            var stats = new GameStats(fakeTime);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            string target = "hello world";
            string input = "hello";

            foreach (var (c, i) in target.Zip(input))
            {
                if (c == i)
                {
                    stats.LogKeystroke(c, KeystrokeType.Correct);
                }
                else
                {
                    stats.LogKeystroke(i, KeystrokeType.Incorrect);
                }
            }

            await Assert.That(stats.WordsPerMinute).IsEqualTo(60);
        }
    }
}
```

// File: src\Typical.Tests\TUI\LayoutFactoryTests.cs

```cs
// using Spectre.Console;
// using Spectre.Console.Rendering;
// using Typical.TUI.Runtime;
// using Typical.TUI.Settings;

// namespace Typical.Tests.TUI;

// public class LayoutFactoryTests
// {
//     private readonly IRenderable _testRenderable = new Text("Test Content");

//     [Test]
//     public async Task Constructor_WhenGivenNullConfiguration_DoesNotThrow()
//     {
//         // Arrange & Act
//         var factoryAction = () => new LayoutFactory(null!);

//         // Assert
//         await Assert.That(factoryAction.Invoke).ThrowsNothing();
//     }

//     [Test]
//     public async Task GetContentFor_WhenContentExistsInConfiguration_ReturnsLayoutWithCorrectNameAndRenderable()
//     {
//         // Arrange
//         var config = new RuntimeLayoutDict();
//         var factory = new LayoutFactory(config);

//         // Act
//         var resultLayout = factory.GetContentFor(LayoutSection.Header);

//         // Assert
//         await Assert.That(resultLayout).IsNotNull();
//         await Assert.That(LayoutSection.Header.Value).IsEqualTo(resultLayout.Name);
//         // await Assert.That(_testRenderable, resultLayout.Renderable).AreSame();
//     }

//     [Test]
//     public async Task GetContentFor_WhenContentDoesNotExistInConfiguration_ReturnsLayoutWithCorrectNameAndNullRenderable()
//     {
//         // Arrange
//         var config = LayoutConfiguration.Default;
//         var factory = new LayoutFactory(config);

//         // Act
//         var resultLayout = factory.GetContentFor(LayoutSection.Footer);

//         // Assert
//         await Assert.That(resultLayout).IsNotNull();
//         await Assert.That(LayoutSection.Footer.Value).IsEqualTo(resultLayout.Name);
//         // await Assert.That(resultLayout.Renderable).IsNull(); // TODO: Use IAnsiConsole TestConsole
//     }

//     [Test]
//     public async Task BuildClassicFocus_WithEmptyConfiguration_BuildsSuccessfully()
//     {
//         // Arrange
//         var factory = new LayoutFactory(LayoutConfiguration.Default);

//         // Act
//         var layout = factory.Build(LayoutName.Dashboard);

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(LayoutName.Dashboard.Value).IsEqualTo(layout.Name);
//     }

//     [Test]
//     public async Task Build_WhenRootLayoutNotInPresets_ReturnsEmptyRootLayout()
//     {
//         // Arrange
//         var factory = new LayoutFactory(new RuntimeLayoutDict());

//         // Act
//         var layout = factory.Build(LayoutName.Root);

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(layout.Name).IsEqualTo(LayoutName.Root.Value);
//         await Assert.That(layout.Children).IsEmpty();
//     }

//     [Test]
//     public async Task Build_ReturnsRootLayout()
//     {
//         // Arrange
//         var factory = new LayoutFactory();

//         // Act
//         var layout = factory.Build();

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(LayoutSection.Root.Value).IsEqualTo(layout.Name);
//         // await Assert.That(layout.Renderable).IsNull(); // TODO: Use IAnsiConsole TestConsole
//     }
// }
```

// File: src\Typical.Tests\TUI\ThemeSettingsBindingTests.cs

```cs
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using TUnit;
// using Typical.TUI.Runtime;
// using Typical.TUI.Settings;

// namespace Typical.TUI.Tests;

// public class ThemeSettingsBindingTests
// {
//     [Test]
//     public async Task Can_Bind_ThemeSettings_With_LayoutName_Keys()
//     {
//         // Arrange: fake config JSON in memory
//         var json =
//             @"
//         {
//           ""Theme"": {
//             ""Header"": {
//               ""PanelHeader"": { ""Text"": ""HeaderText"" }
//             },
//             ""TypingArea"": {
//               ""PanelHeader"": { ""Text"": ""TypingText"" }
//             }
//           }
//         }";

//         var configuration = new ConfigurationBuilder()
//             .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
//             .Build();

//         // Act: bind into our strongly-typed ThemeSettings
//         var themeSettings = configuration.GetSection("Theme").Get<RuntimeTheme>();

//         // Assert: dictionary has LayoutName keys, not strings
//         await Assert.That(themeSettings).IsNotNull();
//         await Assert.That(themeSettings!.ContainsKey(LayoutSection.Header)).IsTrue();
//         await Assert.That(themeSettings!.ContainsKey(LayoutSection.TypingArea)).IsTrue();

//         // And check values came through
//         await Assert
//             .That(themeSettings[LayoutSection.Header].PanelHeader?.Text)
//             .IsEqualTo("HeaderText");
//         await Assert
//             .That(themeSettings[LayoutSection.TypingArea].PanelHeader?.Text)
//             .IsEqualTo("TypingText");
//     }
// }
```

// File: src\Typical.Tests\TUI\ThemeTests.cs

```cs
using Spectre.Console;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;

public class ThemeTests
{
    // --- Basic Styling Tests ---

    [Test]
    public async Task Apply_WithSpecificStyle_SetsPanelBorderStyle()
    {
        // Arrange
        var theme = new RuntimeTheme
        {
            {
                LayoutSection.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "Blue" },
                }
            },
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", theme } };
        var manager = new ThemeManager(themeDict);
        var layoutName = LayoutSection.From("TestArea");

        // Act
        var panel = new Panel("");
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle).IsNotNull();
        await Assert.That(panel.BorderStyle!.Foreground).IsEqualTo(Color.Blue);
    }

    [Test]
    public async Task Apply_WithSpecificStyle_SetsPanelHeader()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutSection.From("TestArea"),
                new ElementStyle { PanelHeader = new PanelHeaderSettings { Text = "Hello" } }
            },
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };
        var manager = new ThemeManager(themeDict);

        var panel = new Panel("");
        var layoutName = LayoutSection.From("TestArea");

        // Act
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.Header).IsNotNull();
        await Assert.That(panel.Header!.Text).IsEqualTo("Hello");
    }

    [Test]
    public async Task Apply_WithHexColor_CorrectlyParsesAndSetsColor()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutSection.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "#FF00FF" },
                }
            },
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };

        var manager = new ThemeManager(themeDict);
        var panel = new Panel("");
        var layoutName = LayoutSection.From("TestArea");

        // Act
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Foreground).IsEqualTo(new Color(255, 0, 255));
    }

    [Test]
    public async Task Apply_WithDecoration_CorrectlyParsesAndSetsDecoration()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutSection.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { Decoration = "Underline" },
                }
            },
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };

        var manager = new ThemeManager(themeDict);
        var panel = new Panel("");
        var layoutName = LayoutSection.From("TestArea");

        // Act
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Decoration).IsEqualTo(Decoration.Underline);
    }

    // --- Fallback and Edge Case Tests ---

    [Test]
    public async Task Apply_WhenStyleIsMissing_FallsBackToDefaultStyle()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            // Note: "TestArea" is missing, but "Default" is present.
            {
                LayoutSection.From("Default"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "Red" },
                }
            },
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };

        var manager = new ThemeManager(themeDict);
        var panel = new Panel("");
        var layoutName = LayoutSection.From("TestArea"); // Requesting a style that doesn't exist

        // Act
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Foreground).IsEqualTo(Color.Red);
    }

    [Test]
    public async Task Apply_WhenNoSpecificOrDefaultStyle_DoesNotChangePanel()
    {
        // Arrange
        var settings = new RuntimeTheme(); // Completely empty settings
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };

        var manager = new ThemeManager(themeDict);
        var originalPanel = new Panel("");
        // Manually set a border to ensure it doesn't get overwritten
        originalPanel.BorderStyle = new Style(Color.Green);

        // Act
        manager.Apply(originalPanel, LayoutSection.From("NonExistent"));

        // Assert
        // The panel's style should be unchanged from its original state.
        await Assert.That(originalPanel.BorderStyle?.Foreground).IsEqualTo(Color.Green);
        await Assert.That(originalPanel.Header).IsNull();
    }

    [Test]
    public async Task Apply_WithOnlyPartialStyleInfo_AppliesOnlyWhatIsProvided()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutSection.From("TestArea"),
                new ElementStyle { BorderStyle = new BorderStyleSettings { Decoration = "Bold" } }
            },
            // Note: ForegroundColor and PanelHeader are missing from the config.
        };
        var themeDict = new Dictionary<string, RuntimeTheme> { { "default", settings } };

        var manager = new ThemeManager(themeDict);
        var panel = new Panel("");
        var layoutName = LayoutSection.From("TestArea");

        // Act
        manager.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle).IsNotNull();
        // Foreground should be the default, not null.
        await Assert.That(panel.BorderStyle!.Foreground).IsEqualTo(Color.Default);
        await Assert.That(panel.BorderStyle.Decoration).IsEqualTo(Decoration.Bold);
        await Assert.That(panel.Header).IsNull(); // Header should not have been set.
    }

    // NOTE: The `Alignment` properties are not directly testable on the `Panel` itself,
    // because the `Apply` method returns a *new wrapper object* (`Align`) when alignment is set.
    // Testing this would require checking the type of the returned object, which is
    // more complex and often considered an implementation detail. For now, testing the
    // direct mutations of the panel provides excellent coverage of the core logic.
}
```

// File: src\Typical.Tests\GameEngineTests.cs

```cs
using Typical.Core;

namespace Typical.Tests;

public class TypicalGameTests
{
    private readonly MockTextProvider _mockTextProvider;
    private readonly GameOptions_defaultOptions;
    private readonly GameOptions _strictOptions;

    public TypicalGameTests()
    {
        // This runs before each test, ensuring a clean state.
        _mockTextProvider = new MockTextProvider();
        _defaultOptions = new GameOptions();
        _strictOptions = new GameOptions { ForbidIncorrectEntries = true };
    }

    // --- StartNewGame Tests ---

    [Test]
    public async Task StartNewGame_Always_LoadsTextFromProvider()
    {
        // Arrange
        var expectedText = "This is a test.";
        _mockTextProvider.SetText(expectedText);
        var game = new GameEngine(_mockTextProvider, _defaultOptions);

        // Act
        await game.StartNewGame();

        // Assert
        await Assert.That(game.TargetText).IsEqualTo(expectedText);
    }

    [Test]
    public async Task StartNewGame_WhenGameWasAlreadyInProgress_ResetsState()
    {
        // Arrange
        _mockTextProvider.SetText("some text");
        var game = new GameEngine(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();

        // Simulate playing the game
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        game.ProcessKeyPress(
            new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)
        );
        await Assert.That(game.IsOver).IsTrue();
        await Assert.That(game.UserInput).IsNotEmpty();

        // Act
        _mockTextProvider.SetText("new text");
        await game.StartNewGame();

        // Assert
        await Assert.That(game.IsOver).IsFalse();
        await Assert.That(game.UserInput).IsEmpty();
        await Assert.That(game.TargetText).IsEqualTo("new text");
    }

    // --- ProcessKeyPress Tests ---

    [Test]
    public async Task ProcessKeyPress_EscapeKey_EndsGameAndReturnsFalse()
    {
        // Arrange
        var game = new GameEngine(_mockTextProvider, _defaultOptions);

        // Act
        var result = game.ProcessKeyPress(
            new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)
        );

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(game.IsOver).IsTrue();
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceKey_RemovesLastCharacter()
    {
        // Arrange
        var game = new GameEngine(_mockTextProvider, _defaultOptions);
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        game.ProcessKeyPress(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));
        await Assert.That(game.UserInput).IsEqualTo("ab");

        // Act
        game.ProcessKeyPress(
            new ConsoleKeyInfo(
                (char)ConsoleKey.Backspace,
                ConsoleKey.Backspace,
                false,
                false,
                false
            )
        );

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceOnEmptyInput_DoesNothing()
    {
        // Arrange
        var game = new GameEngine(_mockTextProvider, _defaultOptions);
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(
            new ConsoleKeyInfo(
                (char)ConsoleKey.Backspace,
                ConsoleKey.Backspace,
                false,
                false,
                false
            )
        );

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_WhenGameIsCompleted_SetsIsOverToTrue()
    {
        // Arrange
        _mockTextProvider.SetText("hi");
        var game = new GameEngine(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        game.ProcessKeyPress(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("hi");
        await Assert.That(game.IsOver).IsTrue();
    }

    // --- GameOptions: ForbidIncorrectEntries Tests ---

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndCorrectKey_AppendsCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new GameEngine(_mockTextProvider, _strictOptions);
        await game.StartNewGame();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndIncorrectKey_DoesNotAppendCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new GameEngine(_mockTextProvider, _strictOptions);
        await game.StartNewGame();
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_InDefaultModeAndIncorrectKey_AppendsCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new GameEngine(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("x");
    }
}
```

// File: src\Typical.Tests\MarkupGeneratorTests.cs

```cs
using Typical; // Your project's namespace

public class MarkupGeneratorTests
{
    private readonly MarkupGenerator _generator;

    public MarkupGeneratorTests()
    {
        // Create a new instance for each test to ensure isolation.
        _generator = new MarkupGenerator();
    }

    // --- Core Scenarios ---

    [Test]
    public async Task BuildMarkupOptimized_AllCorrectlyTyped_ReturnsFullyCorrectMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hello world";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[default on green]Hello world[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_PartiallyTypedAndCorrect_ReturnsCorrectAndUntypedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hello";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[default on green]Hello[/][grey] world[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_WithErrors_ReturnsMixedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hellx worlb"; // Two errors

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // "Hell" is correct, "o" is incorrect, " worl" is correct, "d" is incorrect.
        var expected =
            "[default on green]Hell[/][red on grey15]o[/][default on green] worl[/][red on grey15]d[/]";
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task BuildMarkupOptimized_NothingTyped_ReturnsFullyUntypedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[grey]Hello world[/]");
    }

    // --- Edge Cases ---

    [Test]
    public async Task BuildMarkupOptimized_EmptyTarget_ReturnsEmptyMarkup()
    {
        // Arrange
        var target = "";
        var typed = "some input";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task BuildMarkupOptimized_UserTypedExtraCharacters_ShowsExtraCharsAsIncorrect()
    {
        // Arrange
        var target = "Hello";
        var typed = "Hello world";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // "Hello" is correct, " world" is the extra incorrect part.
        var expected = "[default on green]Hello[/][red on grey15] world[/]";
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task BuildMarkupOptimized_AllCharactersIncorrect_ReturnsFullyIncorrectMarkup()
    {
        // Arrange
        var target = "abcde";
        var typed = "fghij";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[red on grey15]abcde[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_TargetContainsMarkupCharacters_EscapesThemCorrectly()
    {
        // Arrange
        var target = "[[Hello]]";
        var typed = "[[Hello]]";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // The generator should escape the brackets so Spectre.Console doesn't interpret them.
        await Assert.That(result).IsEqualTo("[default on green][[[[Hello]]]][/]");
    }
}
```

// File: src\Typical.Tests\MockTextProvider.cs

```cs
using Typical.Core.Text;

namespace Typical.Tests;

public class MockTextProvider : ITextProvider
{
    private string _textToReturn = string.Empty;

    public void SetText(string text)
    {
        _textToReturn = text;
    }

    public Task<string> GetTextAsync()
    {
        // Task.FromResult is the perfect way to simulate an
        // async operation that completes immediately.
        return Task.FromResult(_textToReturn);
    }
}
```
