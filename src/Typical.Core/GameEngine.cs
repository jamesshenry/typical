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
    private string[] _targetGraphemes = [];
    private readonly List<string> _userInputGraphemes = [];
    private readonly GameOptions _gameOptions;

    // TODO: Add HeatmapCollector
    private readonly ILogger<GameEngine> _logger;
    private KeystrokeType[] _charStates = [];
    private readonly StringBuilder _userInputBuffer = new StringBuilder();

    public GameEngine(GameOptions gameOptions, ILogger<GameEngine> logger)
    {
        _gameOptions = gameOptions;
        Stats = new GameStats();
        _logger = logger;
    }

    public IReadOnlyList<KeystrokeType> CharacterStates => _charStates;
    public GameStats Stats { get; private set; }
    public string TargetText { get; private set; } = string.Empty;

    public string UserInput => _userInputBuffer.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && Stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public GameSnapshot CreateSnapshot() => Stats.CreateSnapshot(TargetText, UserInput, IsOver);

    public bool ProcessKeyPress(string input, bool isBackspace)
    {
        if (!IsRunning && !IsOver && !isBackspace)
        {
            Stats.Start();
            CoreLogs.GameStarting(_logger);
        }

        int currentPos = _userInputGraphemes.Count;

        if (isBackspace)
        {
            if (currentPos > 0)
            {
                string lastGrapheme = _userInputGraphemes[^1];
                _userInputGraphemes.RemoveAt(currentPos - 1);
                _userInputBuffer.Remove(
                    _userInputBuffer.Length - lastGrapheme.Length,
                    lastGrapheme.Length
                );
                _charStates[currentPos - 1] = KeystrokeType.Untyped;
                Stats.RecordBackspace();
            }
            return true;
        }

        if (currentPos >= _targetGraphemes.Length)
            return false;

        // var type = DetermineKeystrokeType(c);
        string normalizedInput = input.Normalize(NormalizationForm.FormC);
        bool isCorrect = normalizedInput == _targetGraphemes[currentPos];
        Stats.RecordKey(normalizedInput, _charStates[currentPos]);

        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInputGraphemes.Add(normalizedInput);
            _userInputBuffer.Append(normalizedInput);
            _charStates[currentPos] = isCorrect ? KeystrokeType.Correct : KeystrokeType.Incorrect;
        }

        CheckEndCondition();
        return true;
    }

    private void CheckEndCondition()
    {
        if (_userInputGraphemes.Count == _targetGraphemes.Length)
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

        _userInputGraphemes.Clear();
        _userInputBuffer.Clear();

        _charStates = new KeystrokeType[_targetGraphemes.Length];
        Array.Fill(_charStates, KeystrokeType.Untyped);

        IsOver = false;
        Stats = new GameStats();
    }
}
