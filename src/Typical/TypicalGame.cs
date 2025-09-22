using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
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

    public TypicalGame(
        GameEngine engine,
        ThemeManager theme,
        MarkupGenerator markupGenerator,
        LayoutFactory layoutFactory,
        IAnsiConsole console
    )
    {
        _engine = engine;
        _theme = theme;
        _markupGenerator = markupGenerator;
        _layoutFactory = layoutFactory;
        _console = console;
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
                    bool needsTypingRefresh = false;
                    bool needsStatsRefresh = false;

                    if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
                    {
                        lastWidth = Console.WindowWidth;
                        lastHeight = Console.WindowHeight;
                        needsTypingRefresh = true;
                        needsStatsRefresh = true;
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (!_engine.ProcessKeyPress(key))
                            break;

                        needsTypingRefresh = true;
                    }

                    if (_engine.IsRunning && statsTimer.ElapsedMilliseconds > statsUpdateIntervalMs)
                    {
                        needsStatsRefresh = true;
                        statsTimer.Restart(); // Reset the timer
                    }

                    if (needsTypingRefresh)
                    {
                        typingArea.Update(CreateTypingArea());
                    }
                    if (needsStatsRefresh)
                    {
                        statsArea.Update(CreateGameInfoArea());
                    }

                    if (needsTypingRefresh || needsStatsRefresh)
                    {
                        ctx.Refresh();
                    }

                    if (_engine.IsOver)
                    {
                        typingArea.Update(CreateTypingArea());
                        statsArea.Update(CreateGameInfoArea());
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
