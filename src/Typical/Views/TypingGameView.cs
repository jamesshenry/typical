using System.ComponentModel;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical;
using Typical.Binding;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;
using Typical.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

public class TypingGameView : BindableView<TypingViewModel>
{
    private readonly Label _statsLabel;

    public TypingGameView(TypingViewModel viewModel)
        : base(viewModel)
    {
        CanFocus = true;
        X = Pos.Center();
        Y = Pos.Center();
        Width = viewModel.TargetText.Length;
        Height = 1;
        _statsLabel = new Label { Y = Pos.AnchorEnd(1) };
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        string text = ViewModel.TargetText;

        for (int i = 0; i < text.Length; i++)
        {
            var status = ViewModel.GetStatus(i);

            Attribute color = status switch
            {
                KeystrokeType.Correct => new Attribute(Color.Green, Color.Black),
                KeystrokeType.Incorrect => new Attribute(Color.White, Color.Red),
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
        ViewModel.ProcessInput((char)key.AsRune.Value, false);
        if (key == Key.Backspace)
        {
            if (ViewModel.TypedText.Length > 0)
                ViewModel.TypedText = ViewModel.TypedText[..^1];
            return true;
        }

        if (!Rune.IsControl(key.AsRune))
        {
            ViewModel.TypedText += key.AsRune.ToString();
            return true;
        }

        return base.OnKeyDown(key);
    }

    protected override void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SetNeedsDraw();
    }

    protected override void SetupBindings()
    {
        var binding = _statsLabel.BindTextOneWay(
            ViewModel,
            () => $"WPM: {ViewModel.Wpm} | Acc: {ViewModel.Accuracy}",
            nameof(ViewModel.Wpm)
        );

        BindingContext.AddBinding(binding);
    }
}
