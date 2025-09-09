using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
using Typical.TUI;

namespace Typical;

public class GameRunner
{
    private readonly MarkupGenerator _markupGenerator;
    private readonly TypicalGame _engine;
    private readonly LayoutConfiguration _layoutConfiguration;

    public GameRunner(TypicalGame engine, LayoutConfiguration layoutConfiguration)
    {
        _engine = engine;
        _markupGenerator = new MarkupGenerator();
        _layoutConfiguration = layoutConfiguration;
    }

    public void Run()
    {
        var layoutFactory = new LayoutFactory(_layoutConfiguration);
        var layout = layoutFactory.BuildDashboard();

        AnsiConsole
            .Live(layout)
            .Start(ctx =>
            {
                var centerLayout = layout[LayoutName.TypingArea.Value];
                centerLayout.Update(CreateGamePanel());
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
                        centerLayout.Update(CreateGamePanel());
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

    private IRenderable CreateGamePanel()
    {
        var originalPanelInfo = _layoutConfiguration.Renderables[LayoutName.TypingArea];
        if (originalPanelInfo is null)
            return new Panel("No content configured for TypingArea").NoBorder();
        var markup = _markupGenerator.BuildMarkupOptimized(_engine.TargetText, _engine.UserInput);
        var newPanel = new Panel(markup);
        Panel originalPanel = (originalPanelInfo.Content as Panel)!;
        newPanel.BorderStyle = originalPanel.BorderStyle;
        newPanel.Header = originalPanel.Header;
        if (originalPanelInfo.AlignmentFunc != null)
        {
            return originalPanelInfo.AlignmentFunc.Invoke(
                newPanel,
                originalPanelInfo.VerticalAlignment
            )!;
        }
        return newPanel;
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
