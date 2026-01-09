using System.Text;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngine
{
    private readonly StringBuilder _userInput = new();
    private readonly ITextProvider _textProvider;
    private readonly GameOptions _gameOptions;
    public GameStats Stats { get; }

    // TODO: Add HeatmapCollector
    private readonly ILogger<GameEngine> _logger;

    public GameEngine(
        ITextProvider textProvider,
        GameOptions gameOptions,
        ILogger<GameEngine> logger
    )
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        Stats = new GameStats();
        _logger = logger;
    }

    public string TargetText { get; private set; } = string.Empty;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && Stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public bool ProcessKeyPress(char c, bool isBackspace)
    {
        if (isBackspace)
        {
            if (_userInput.Length > 0)
            {
                _userInput.Remove(_userInput.Length - 1, 1);
                Stats.RecordBackspace();
            }
            return true;
        }

        var type = DetermineKeystrokeType(c);
        Stats.RecordKey(c, type);

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(c);
        }

        CheckEndCondition();
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
            Stats.Stop();
            CoreLogs.GameFinished(_logger);
        }
    }

    public async Task StartNewGame()
    {
        CoreLogs.GameStarting(_logger);
        var text = await _textProvider.GetTextAsync();
        TargetText = text.Text;
        Stats.Start();
        _userInput.Clear();
        IsOver = false;
        PublishStateUpdate();
    }

    private void PublishStateUpdate()
    {
        CoreLogs.PublishingState(_logger);
        var snapShot = Stats.CreateSnapshot();
        var stateEvent = new GameStateUpdatedEvent(TargetText, UserInput, snapShot, IsOver);
    }
}
