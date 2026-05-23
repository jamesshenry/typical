using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class TypingTest
{
    private readonly TypingBuffer _userInput = new();
    private string[] _targetGraphemes = [];
    private readonly GameOptions _gameOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TypingTest> _logger;
    public event EventHandler<TestResult>? OnTestFinished;

    public TypingTest(
        GameOptions gameOptions,
        ILogger<TypingTest> logger,
        TimeProvider timeProvider
    )
    {
        _gameOptions = gameOptions;
        _timeProvider = timeProvider;
        Stats = new Statistics.Statistics(_timeProvider);
        _logger = logger;
    }

    internal Statistics.Statistics Stats { get; private set; }
    public string TargetText { get; private set; } = string.Empty;

    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && Stats.IsRunning;

    public TextSample SampleNormalized { get; private set; } = TextSample.Empty;

    public TestSnapshot CreateSnapshot() => Stats.CreateSnapshot();

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
                _userInput.Pop();

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
            CheckEndCondition();
        }

        return true;
    }

    private void CheckEndCondition()
    {
        if (_userInput.GraphemeCount == _targetGraphemes.Length)
        {
            if (_gameOptions.Require100Accuracy && _userInput.ToString() != TargetText)
            {
                return;
            }

            IsOver = true;
            Stats.Stop();
            var snapshot = Stats.CreateSnapshot();
            var result = new TestResult(
                DateTime.UtcNow,
                snapshot.WPM,
                snapshot.Accuracy,
                snapshot.ElapsedTime,
                SampleNormalized,
                Stats.Keystrokes
            );

            OnTestFinished?.Invoke(this, result);
        }
    }

    public void LoadText(TextSample sample)
    {
        TargetText = sample.Text.Normalize(NormalizationForm.FormC);
        SampleNormalized = sample with { Text = sample.Text.Normalize(NormalizationForm.FormC) };

        List<string> list = [];
        var enumerator = StringInfo.GetTextElementEnumerator(TargetText);
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.GetTextElement());
        }

        _targetGraphemes = list.ToArray();
        _userInput.Clear();

        IsOver = false;
        Stats = new Statistics.Statistics(_timeProvider);
    }

    internal KeystrokeType GetStatus(int index)
    {
        if (index >= _userInput.GraphemeCount)
            return KeystrokeType.Untyped;

        return _userInput.GetGraphemeAt(index) == _targetGraphemes[index]
            ? KeystrokeType.Correct
            : KeystrokeType.Incorrect;
    }
}
