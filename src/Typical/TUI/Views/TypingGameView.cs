using System.ComponentModel;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Typical;
using Typical.Core.ViewModels;
using Typical.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

public class TypingGameView : BindableView<TypingViewModel>
{
    private readonly TypingViewModel _viewModel;

    public TypingGameView(TypingViewModel viewModel)
        : base(viewModel)
    {
        _viewModel = viewModel;
        CanFocus = true;

        X = Pos.Center();
        Y = Pos.Center();
        Width = viewModel.TargetText.Length;
        Height = 1;
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

    protected override void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SetNeedsDraw();
    }

    protected override void SetupBindings() { }
}
