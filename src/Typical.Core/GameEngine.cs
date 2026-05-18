using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngine
{
    private readonly TypingBuffer _userInput = new();
    private string[] _targetGraphemes = [];
    private readonly GameOptions _gameOptions;

    // TODO: Add HeatmapCollector
    private readonly ILogger<GameEngine> _logger;
    private KeystrokeType[] _charStates = [];

    public GameEngine(GameOptions gameOptions, ILogger<GameEngine> logger)
    {
        _gameOptions = gameOptions;
        Stats = new GameStats();
        _logger = logger;
    }

    public IReadOnlyList<KeystrokeType> CharacterStates => _charStates;
    internal GameStats Stats { get; private set; }
    public string TargetText { get; private set; } = string.Empty;

    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && Stats.IsRunning;

    public GameSnapshot CreateSnapshot() => Stats.CreateSnapshot();

    public bool ProcessKeyPress(string input, bool isBackspace)
    {
        if (!IsRunning && !IsOver && !isBackspace)
        {
            Stats.Start();
            CoreLogs.GameStarting(_logger);
        }

        if (isBackspace)
        {
            if (_userInput.GraphemeCount > 0)
            {
                int indexToReset = _userInput.GraphemeCount - 1;
                _userInput.Pop();

                _charStates[indexToReset] = KeystrokeType.Untyped;
                Stats.RecordBackspace();
            }
            return true;
        }

        int currentPos = _userInput.GraphemeCount;
        if (currentPos >= _targetGraphemes.Length)
            return false;

        string normalizedInput = input.Normalize(NormalizationForm.FormC);
        bool isCorrect = normalizedInput == _targetGraphemes[currentPos];
        var type = isCorrect ? KeystrokeType.Correct : KeystrokeType.Incorrect;

        Stats.RecordKey(normalizedInput, type);

        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Push(normalizedInput);
            _charStates[currentPos] = type;
            CheckEndCondition();
        }

        return true;
    }

    private void CheckEndCondition()
    {
        if (_userInput.GraphemeCount == _targetGraphemes.Length)
        {
            bool hasErrors = _charStates.Any(s => s == KeystrokeType.Incorrect);
            if (!hasErrors || !_gameOptions.Require100Accuracy)
            {
                IsOver = true;
                Stats.Stop();
            }
        }
    }

    public void LoadText(TextSample sample)
    {
        TargetText = sample.Text.Normalize(NormalizationForm.FormC);

        List<string> list = [];
        var enumerator = StringInfo.GetTextElementEnumerator(TargetText);
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.GetTextElement());
        }

        _targetGraphemes = list.ToArray();
        _userInput.Clear();

        _charStates = new KeystrokeType[_targetGraphemes.Length];
        Array.Fill(_charStates, KeystrokeType.Untyped);

        IsOver = false;
        Stats = new GameStats();
    }
}
