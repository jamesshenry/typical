using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.TUI;

namespace Typical;

public class TypicalGame
{
    private readonly MarkupGenerator _markupGenerator;

    private readonly string _targetText;
    private readonly StringBuilder _userInput;
    private readonly GameOptions _gameOptions;
    private readonly LayoutConfiguration _layoutConfiguration;

    public TypicalGame(string targetText)
        : this(targetText, new GameOptions(), LayoutConfiguration.Default) { }

    public TypicalGame(string targetText, GameOptions gameOptions)
        : this(targetText, gameOptions, LayoutConfiguration.Default) { }

    public TypicalGame(
        string targetText,
        GameOptions gameOptions,
        LayoutConfiguration layoutConfiguration
    )
    {
        _targetText = targetText;
        _gameOptions = gameOptions;
        _layoutConfiguration = layoutConfiguration;
        _markupGenerator = new MarkupGenerator();
        _userInput = new StringBuilder();
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
                        if (!HandleInput(key))
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

                    if (_userInput.ToString() == _targetText)
                    {
                        Thread.Sleep(500);
                        break;
                    }

                    Thread.Sleep(1000 / _gameOptions.TargetFrameRate);
                }
            });

        DisplaySummary();
    }

    private bool HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
            return false;

        if (key.Key == ConsoleKey.Backspace && _userInput.Length > 0)
        {
            _userInput.Remove(_userInput.Length - 1, 1);
        }
        else if (!char.IsControl(key.KeyChar))
        {
            if (_gameOptions.ForbidIncorrectEntries)
            {
                int currentPos = _userInput.Length;
                if (currentPos < _targetText.Length && key.KeyChar == _targetText[currentPos])
                {
                    _userInput.Append(key.KeyChar);
                }
            }
            else
            {
                _userInput.Append(key.KeyChar);
            }
        }

        return true;
    }

    private IRenderable CreateGamePanel()
    {
        var markup = _markupGenerator.BuildMarkupOptimized(_targetText, _userInput.ToString());

        var panel = new Panel(markup).Header("Typing Area").BorderColor(Color.Blue);
        return Align.Center(panel, VerticalAlignment.Middle);
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
