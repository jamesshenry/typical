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

    private KeystrokeType[] _charStates = [];
    public IReadOnlyList<KeystrokeType> CharacterStates => _charStates;

    public GameEngine(
        ITextProvider textProvider,
        GameOptions gameOptions,
        ILogger<GameEngine> logger
    )
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        _gameOptions.ForbidIncorrectEntries = true;
        Stats = new GameStats();
        _logger = logger;
    }

    public string TargetText { get; private set; } = string.Empty;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsInitialized { get; private set; }

    public bool IsRunning => !IsOver && Stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public bool ProcessKeyPress(char c, bool isBackspace)
    {
        int currentPos = _userInput.Length;

        if (isBackspace)
        {
            if (currentPos > 0)
            {
                _userInput.Remove(currentPos - 1, 1);
                _charStates[currentPos - 1] = KeystrokeType.Untyped;
                Stats.RecordBackspace();
            }
            return true;
        }

        if (currentPos >= TargetText.Length)
            return false;

        var type = DetermineKeystrokeType(c);

        _charStates[currentPos] = type;
        Stats.RecordKey(c, type);

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(c);
        }
        else { }

        CheckEndCondition();
        return true;
    }

    private KeystrokeType DetermineKeystrokeType(char inputChar)
    {
        int currentPos = _userInput.Length;
        return currentPos >= TargetText.Length ? KeystrokeType.Extra
            : inputChar == TargetText[currentPos] ? KeystrokeType.Correct
            : KeystrokeType.Incorrect;
    }

    private void CheckEndCondition()
    {
        if (_userInput.ToString().Equals(TargetText))
        {
            IsOver = true;
            IsInitialized = false;
            Stats.Stop();
            CoreLogs.GameFinished(_logger);
        }
    }

    public void StartNewGame()
    {
        if (IsInitialized)
        {
            CoreLogs.GameStarting(_logger);
            Stats.Start();
            PublishStateUpdate();
        }
        else
        {
            throw new Exception();
        }
    }

    private void PublishStateUpdate()
    {
        CoreLogs.PublishingState(_logger);
        var snapShot = Stats.CreateSnapshot();
        var stateEvent = new GameStateUpdatedEvent(TargetText, UserInput, snapShot, IsOver);
    }

    internal async Task InitializeAsync()
    {
        var text = await _textProvider.GetTextAsync();
        TargetText = text.Text;
        _userInput.Clear();

        _charStates = new KeystrokeType[TargetText.Length];
        Array.Fill(_charStates, KeystrokeType.Untyped);

        IsOver = false;
        IsInitialized = true;
    }
}
