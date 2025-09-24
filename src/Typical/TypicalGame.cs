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
    private readonly GameEngine _engine;
    private readonly ThemeManager _theme;
    private readonly LayoutFactory _layoutFactory;
    private readonly IAnsiConsole _console;
    private bool _needsTypingRefresh;
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
        var markup = _markupGenerator.BuildMarkupOptimized(_engine.TargetText, _engine.UserInput);
        return _theme.Apply(markup, LayoutSection.TypingArea);
    }

    private IRenderable CreateHeader()
    {
        return _theme.Apply(new Markup("Typical - A Typing Tutor"), LayoutSection.Header);
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
