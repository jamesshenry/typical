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

    public GameEngine(
        ITextProvider textProvider,
        IEventAggregator eventAggregator,
        GameOptions gameOptions,
        GameStats stats,
        ILogger<GameEngine> logger
    )
    {
        _userInput = new StringBuilder();
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _eventAggregator = eventAggregator;
        _gameOptions = gameOptions;
        _stats = stats;
        _logger = logger;
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
