using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;

namespace Typical.TUI.Views;

public class GameView : IView
{
    private readonly MarkupGenerator _markupGenerator;

    private GameEngine _engine = default!;
    private readonly IGameEngineFactory _gameEngineFactory;
    private readonly ThemeManager _theme;
    private readonly LayoutFactory _layoutFactory;
    private readonly IAnsiConsole _console;
    private string _targetText = string.Empty;
    private string _userInput = string.Empty;
    private GameStatisticsSnapshot _statistics = GameStatisticsSnapshot.Empty;
    private bool _isGameOver;
    private bool _needsRefresh;

    public GameView(
        IGameEngineFactory gameEngineFactory,
        ThemeManager theme,
        MarkupGenerator markupGenerator,
        LayoutFactory layoutFactory,
        IEventAggregator eventAggregator,
        IAnsiConsole console
    )
    {
        _gameEngineFactory = gameEngineFactory;
        _theme = theme;
        _markupGenerator = markupGenerator;
        _layoutFactory = layoutFactory;
        _console = console;

        eventAggregator.Subscribe<GameStateUpdatedEvent>(OnGameStateUpdated);
    }

    private void OnGameStateUpdated(GameStateUpdatedEvent e)
    {
        // Cache the new state
        _targetText = e.TargetText;
        _userInput = e.UserInput;
        _statistics = e.Statistics;
        _isGameOver = e.IsOver;

        _needsRefresh = true;
    }

    public async Task RenderAsync()
    {
        await RenderAsync(GameOptions.Default);
    }

    public async Task RenderAsync(GameOptions options)
    {
        _engine = _gameEngineFactory.Create(options);
        var layout = _layoutFactory.Build(LayoutName.Dashboard);
        await _console
            .Live(layout)
            .StartAsync(async ctx =>
            {
                await _engine.StartNewGame();
                var typingArea = layout[LayoutSection.TypingArea.Value];
                var statsArea = layout[LayoutSection.GameInfo.Value];
                var headerArea = layout[LayoutSection.Header.Value];
                typingArea.Update(CreateTypingArea());
                statsArea.Update(CreateGameInfoArea());
                headerArea.Update(CreateHeader());

                ctx.Refresh();

                int lastHeight = Console.WindowHeight;
                int lastWidth = Console.WindowWidth;

                while (!_isGameOver)
                {
                    if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
                    {
                        lastWidth = Console.WindowWidth;
                        lastHeight = Console.WindowHeight;
                        _needsRefresh = true;
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (!_engine.ProcessKeyPress(key))
                            break;
                    }

                    if (_needsRefresh)
                    {
                        layout[LayoutSection.TypingArea.Value].Update(CreateTypingArea());
                        layout[LayoutSection.GameInfo.Value].Update(CreateGameInfoArea());
                        ctx.Refresh();
                        _needsRefresh = false;
                    }

                    if (_isGameOver)
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
        if (_statistics is null)
            return new Text("");

        var grid = new Grid();
        grid.AddColumns([new GridColumn(), new GridColumn()]);
        grid.AddRow("WPM:", $"{_statistics.WordsPerMinute:F1}");
        grid.AddRow("Accuracy:", $"{_statistics.Accuracy:F1}%");
        grid.AddRow("Correct Chars:", $"{_statistics.Chars.Correct}");
        grid.AddRow("Incorrect Chars:", $"{_statistics.Chars.Incorrect}");
        grid.AddRow("Extra Chars:", $"{_statistics.Chars.Extra}");
        grid.AddRow("Elapsed:", $"{_statistics.ElapsedTime:mm\\:ss}");
        return _theme.Apply(grid, LayoutSection.GameInfo);
    }

    private IRenderable CreateTypingArea()
    {
        var markup = _markupGenerator.BuildMarkupOptimized(_targetText, _userInput);
        return _theme.Apply(markup, LayoutSection.TypingArea);
    }

    private IRenderable CreateHeader()
    {
        return _theme.Apply(new Markup("Typical - A Typing Tutor"), LayoutSection.Header);
    }

    private Action<string> DisplaySummary() =>
        summaryString => AnsiConsole.MarkupLineInterpolated($"[bold green]{summaryString}[/]");
}
