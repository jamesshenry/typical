using System.Text;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class TypicalGame
{
    private readonly StringBuilder _userInput;
    private readonly ITextProvider _textProvider;
    private readonly GameOptions _gameOptions;

    public TypicalGame(ITextProvider textProvider)
        : this(textProvider, new GameOptions()) { }

    public TypicalGame(ITextProvider textProvider, GameOptions gameOptions)
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
    private readonly GameStats _stats;

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
        }
        else if (!char.IsControl(key.KeyChar))
        {
            int currentPos = _userInput.Length;
            if (currentPos >= TargetText.Length)
            {
                _stats.LogKeystroke(key.KeyChar, KeystrokeType.Extra);
            }
            else if (key.KeyChar == TargetText[currentPos])
            {
                _stats.LogKeystroke(key.KeyChar, KeystrokeType.Correct);
            }
            else
            {
                _stats.LogKeystroke(key.KeyChar, KeystrokeType.Incorrect);
            }

            if (
                !_gameOptions.ForbidIncorrectEntries
                || (currentPos < TargetText.Length && key.KeyChar == TargetText[currentPos])
            )
            {
                _userInput.Append(key.KeyChar);
            }
        }

        if (_userInput.ToString() == TargetText)
        {
            IsOver = true;
            _stats.Stop();
        }

        return true;
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
