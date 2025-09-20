using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
using Typical.TUI;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;

namespace Typical;

public class GameRunner
{
    private readonly MarkupGenerator _markupGenerator;
    private readonly TypicalGame _engine;
    private readonly ThemeManager _theme;
    private readonly LayoutFactory _layoutFactory;
    private readonly IAnsiConsole _console;
    private readonly GameStats _stats;

    public GameRunner(
        TypicalGame engine,
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
        _stats = new GameStats();
    }

    public void Run()
    {
        var layout = _layoutFactory.Build(LayoutName.Dashboard);

        _console
            .Live(layout)
            .Start(ctx =>
            {
                var typingArea = layout[LayoutSection.TypingArea.Value];
                typingArea.Update(CreateTypingArea());
                ctx.Refresh();

                int lastHeight = Console.WindowHeight;
                int lastWidth = Console.WindowWidth;

                while (true)
                {
                    bool needsRefresh = false;

                    if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
                    {
                        lastWidth = Console.WindowWidth;
                        lastHeight = Console.WindowHeight;
                        needsRefresh = true;
                    }

                    if (Console.KeyAvailable)
                    {
                        _stats.Start();
                        var key = Console.ReadKey(true);
                        if (!_engine.ProcessKeyPress(key))
                        {
                            break;
                        }
                        needsRefresh = true;
                    }

                    if (needsRefresh)
                    {
                        typingArea.Update(CreateTypingArea());
                        ctx.Refresh();
                    }

                    if (_engine.IsOver)
                    {
                        Thread.Sleep(500);
                        break;
                    }

                    Thread.Sleep(_engine.TargetFrameDelayMilliseconds);
                }
            });

        DisplaySummary();
    }

    private IRenderable CreateTypingArea()
    {
        var markup = _markupGenerator.BuildMarkupOptimized(_engine.TargetText, _engine.UserInput);
        var panel = new Panel(markup);

        IRenderable applied = _theme.Apply(panel, LayoutSection.TypingArea);
        return applied;
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
