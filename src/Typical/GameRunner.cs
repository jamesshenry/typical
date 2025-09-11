using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
using Typical.TUI;
using Typical.TUI.Settings;

namespace Typical;

public class GameRunner
{
    private readonly MarkupGenerator _markupGenerator;
    private readonly TypicalGame _engine;
    private readonly Theme _theme;
    private readonly IAnsiConsole _console;

    public GameRunner(
        TypicalGame engine,
        Theme theme,
        MarkupGenerator markupGenerator,
        IAnsiConsole console
    )
    {
        _engine = engine;
        _theme = theme;
        _markupGenerator = markupGenerator;
        _console = console;
    }

    public void Run()
    {
        var layoutFactory = new LayoutFactory();
        var layout = layoutFactory.BuildDashboard();

        _console
            .Live(layout)
            .Start(ctx =>
            {
                var typingArea = layout[LayoutName.TypingArea.Value];
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

        _theme.Apply(panel, LayoutName.TypingArea);
        return panel;
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
