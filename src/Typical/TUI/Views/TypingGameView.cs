using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Typical;
using Attribute = Terminal.Gui.Drawing.Attribute;

public class TypingGameView : View
{
    private readonly TypingViewModel _viewModel;

    public TypingGameView(TypingViewModel viewModel)
    {
        _viewModel = viewModel;
        CanFocus = true;

        _viewModel.PropertyChanged += (s, e) => SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        string text = _viewModel.TargetText;

        for (int i = 0; i < text.Length; i++)
        {
            var status = _viewModel.GetStatus(i);

            Attribute color = status switch
            {
                TypingResult.Correct => new Attribute(Color.Green, Color.Black),
                TypingResult.Incorrect => new Attribute(Color.White, Color.Red),
                _ => new Attribute(Color.DarkGray, Color.Black),
            };

            Move(i, 0);

            SetAttribute(color);

            AddRune(new Rune(text[i]));
        }

        var drawnRect = new System.Drawing.Rectangle(0, 0, text.Length, 1);
        context?.AddDrawnRectangle(ViewportToScreen(drawnRect));

        return true;
    }

    protected override bool OnKeyDown(Key key)
    {
        if (key == Key.Backspace)
        {
            if (_viewModel.TypedText.Length > 0)
                _viewModel.TypedText = _viewModel.TypedText[..^1];
            return true;
        }

        if (!Rune.IsControl(key.AsRune))
        {
            _viewModel.TypedText += key.AsRune.ToString();
            return true;
        }

        return base.OnKeyDown(key);
    }
}

public class TypingViewModel : INotifyPropertyChanged
{
    private readonly string _targetText = "The quick brown fox jumped over the lazy dog.";
    private string _typedText = "";

    public string TargetText => _targetText;
    public string TypedText
    {
        get => _typedText;
        set
        {
            if (_typedText != value)
            {
                _typedText = value;
                OnPropertyChanged();
            }
        }
    }

    internal TypingResult GetStatus(int index)
    {
        if (index >= _typedText.Length)
            return TypingResult.Untyped;
        return _typedText[index] == _targetText[index]
            ? TypingResult.Correct
            : TypingResult.Incorrect;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
