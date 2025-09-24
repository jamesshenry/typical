using System.Text;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngine
{
    private readonly StringBuilder _userInput;
    private readonly ITextProvider _textProvider;
    private readonly GameOptions _gameOptions;
    private readonly GameStats _stats;

    public GameEngine(ITextProvider textProvider)
        : this(textProvider, new GameOptions()) { }

    public GameEngine(ITextProvider textProvider, GameOptions gameOptions)
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        _userInput = new StringBuilder();
        _stats = new GameStats();
    }

    public string TargetText { get; private set; } = string.Empty;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public bool IsRunning => !IsOver && _stats.IsRunning;
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public event EventHandler<GameEndedEventArgs>? GameEnded;
    public event EventHandler<GameStateChangedEventArgs>? StateChanged;

    public bool ProcessKeyPress(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
        {
            IsOver = true;
            _stats.Stop();
            return false;
        }

        if (key.Key == ConsoleKey.Backspace && _userInput.Length > 0)
        {
            _userInput.Remove(_userInput.Length - 1, 1);
            _stats.LogCorrection(); // Assuming you have/want this method
            return true;
        }
        if (char.IsControl(key.KeyChar))
        {
            return true; // Ignore other control characters but continue the game
        }
        char inputChar = key.KeyChar;

        KeystrokeType type = DetermineKeystrokeType(inputChar);

        _stats.LogKeystroke(inputChar, type);

        bool isCorrect = type == KeystrokeType.Correct;
        if (!_gameOptions.ForbidIncorrectEntries || isCorrect)
        {
            _userInput.Append(key.KeyChar);
            StateChanged?.Invoke(this, new GameStateChangedEventArgs());
        }

        CheckEndCondition();

        return true;
    }

    private KeystrokeType DetermineKeystrokeType(char inputChar)
    {
        int currentPos = _userInput.Length;

        if (currentPos >= TargetText.Length)
        {
            return KeystrokeType.Extra;
        }

        if (inputChar == TargetText[currentPos])
        {
            return KeystrokeType.Correct;
        }

        return KeystrokeType.Incorrect;
    }

    private void CheckEndCondition()
    {
        if (_userInput.ToString() == TargetText)
        {
            IsOver = true;
            _stats.Stop();

            GameEnded?.Invoke(this, new GameEndedEventArgs(_stats.CreateSnapshot()));
        }
    }

    public async Task StartNewGame()
    {
        TargetText = await _textProvider.GetTextAsync();
        _stats.Start();
        _userInput.Clear();
        IsOver = false;
    }

    public GameStatisticsSnapshot GetGameStatistics()
    {
        return _stats.CreateSnapshot();
    }
}
