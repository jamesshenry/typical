using System.Text;
using System.Threading.Tasks;

namespace Typical.Core;

public class TypicalGame
{
    private readonly StringBuilder _userInput;
    private readonly ITextProvider _textProvider;
    private readonly GameOptions _gameOptions;
    private string _targetText = string.Empty;

    public TypicalGame(ITextProvider textProvider)
        : this(textProvider, new GameOptions()) { }

    public TypicalGame(ITextProvider textProvider, GameOptions gameOptions)
    {
        _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
        _gameOptions = gameOptions;
        _userInput = new StringBuilder();
    }

    public string TargetText => _targetText;
    public string UserInput => _userInput.ToString();
    public bool IsOver { get; private set; }
    public int TargetFrameDelayMilliseconds => 1000 / _gameOptions.TargetFrameRate;

    public bool ProcessKeyPress(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
        {
            IsOver = true;
            return false;
        }

        if (key.Key == ConsoleKey.Backspace && _userInput.Length > 0)
        {
            _userInput.Remove(_userInput.Length - 1, 1);
        }
        else if (!char.IsControl(key.KeyChar))
        {
            if (_gameOptions.ForbidIncorrectEntries)
            {
                int currentPos = _userInput.Length;
                if (currentPos < _targetText.Length && key.KeyChar == _targetText[currentPos])
                {
                    _userInput.Append(key.KeyChar);
                }
            }
            else
            {
                _userInput.Append(key.KeyChar);
            }
        }

        if (_userInput.ToString() == _targetText)
        {
            IsOver = true;
        }

        return true;
    }

    public async Task StartNewGame()
    {
        _targetText = await _textProvider.GetTextAsync();
        _userInput.Clear();
        IsOver = false;
    }
}
