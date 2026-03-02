using System.Diagnostics;
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
    private readonly GameOptions _gameOptions;

    // TODO: Add HeatmapCollector
    private readonly ILogger<GameEngine> _logger;

    private KeystrokeType[] _charStates = [];
    public IReadOnlyList<KeystrokeType> CharacterStates => _charStates;

    public GameEngine(GameOptions gameOptions, ILogger<GameEngine> logger)
    {
        _gameOptions = gameOptions;
        Stats = new GameStats();
        _logger = logger;
    }

    public GameStats Stats { get; private set; }
    public string TargetText { get; private set; } = string.Empty;

    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && Stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public bool ProcessKeyPress(char c, bool isBackspace)
    {
        if (!IsRunning && !IsOver && TargetText.Length > 0 && !isBackspace)
        {
            Stats.Start();
            CoreLogs.GameStarting(_logger);
        }

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
        Stats.RecordKey(c, type);

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(c);
            _charStates[currentPos] = type;
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
        if (_userInput.Length == TargetText.Length)
        {
            if (_userInput.ToString().Equals(TargetText))
            {
                IsOver = true;
                Stats.Stop();
                CoreLogs.GameFinished(_logger);
            }
        }
    }

    public void LoadText(TextSample sample)
    {
        var text = sample.Text;
        TargetText = text;
        _userInput.Clear();
        _charStates = new KeystrokeType[text.Length];
        Array.Fill(_charStates, KeystrokeType.Untyped);

        IsOver = false;
        Stats = new GameStats();
    }
}
