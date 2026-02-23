using System.ComponentModel;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;
using Typical.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

public class TypingGameView : BindableView<TypingViewModel>
{
    private readonly Label _statsLabel;
    private readonly TextFormatter _formatter = new();
    private List<string> _cachedLines = [];

    public TypingGameView(TypingViewModel viewModel)
        : base(viewModel)
    {
        CanFocus = true;
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Percent(80);
        Height = Dim.Percent(50);
        BorderStyle = LineStyle.RoundedDashed;
        Title = nameof(TypingGameView);
        _formatter.WordWrap = true;

        _statsLabel = new Label { Y = Pos.AnchorEnd(1) };
        Add(_statsLabel);
        Initialized += (s, e) => _ = InitializeViewAsync();
        this.Activating += (s, e) =>
        {
            this.SetFocus();
            e.Handled = true; // Prevents the click from reaching MainShell
        };
    }

    protected override void OnSubViewsLaidOut(LayoutEventArgs args)
    {
        base.OnSubViewsLaidOut(args);
        RefreshTextCache();
    }

    private void RefreshTextCache()
    {
        _formatter.Text = ViewModel.TargetText;
        _formatter.ConstrainToWidth = Viewport.Width;
        _formatter.ConstrainToHeight = Viewport.Height;

        _cachedLines = _formatter.GetLines();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (context == null || _cachedLines.Count == 0)
            return true;

        var scheme = this.GetScheme();
        var normalBack = scheme.Normal.Background;

        var correctAttr = new Attribute(Color.Green, normalBack);
        var incorrectAttr = new Attribute(Color.White, Color.Red);
        var untypedAttr = new Attribute(Color.DarkGray, normalBack);

        int globalCharIndex = 0;
        for (int y = 0; y < _cachedLines.Count; y++)
        {
            string lineText = _cachedLines[y];
            Move(0, y);

            for (int x = 0; x < lineText.Length; x++)
            {
                var status = ViewModel.GetStatus(globalCharIndex);

                Attribute color = status switch
                {
                    KeystrokeType.Correct => correctAttr,
                    KeystrokeType.Incorrect => incorrectAttr,
                    _ => untypedAttr,
                };

                SetAttribute(color);
                AddRune(new Rune(lineText[x]));

                globalCharIndex++;
            }
        }

        return true;
    }

    protected override bool OnKeyDown(Key key)
    {
        if (key.IsCtrl || key.IsAlt || key == Key.Tab || key == Key.Esc || key == Key.F4)
        {
            return base.OnKeyDown(key);
        }

        bool isBackspace = key == Key.Backspace;
        Rune rune = key.AsRune;

        if (rune != default || isBackspace)
        {
            char c = isBackspace ? '\0' : (char)rune.Value;
            HandleInput(c, isBackspace);

            return true;
        }

        return base.OnKeyDown(key);
    }

    private void HandleInput(char c, bool isBackspace)
    {
        try
        {
            ViewModel.ProcessInput(c, isBackspace);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Input Error: {ex.Message}");
        }
    }

    protected override void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        App?.Invoke(() =>
        {
            if (e.PropertyName == nameof(ViewModel.TargetText))
            {
                RefreshTextCache();
                SetNeedsLayout();
            }
            SetNeedsDraw();
        });
    }

    protected override void SetupBindings()
    {
        BindingContext.AddBinding(
            ViewModel.BindText(
                nameof(ViewModel.TypedText),
                _statsLabel,
                () =>
                    $"Elapsed: {ViewModel.TimeElapsed} WPM: {ViewModel.Wpm} | Acc: {ViewModel.Accuracy}"
            )
        );
        BindingContext.AddBinding(
            ViewModel.Bind(
                nameof(ViewModel.CharacterStates),
                () => ViewModel.CharacterStates,
                _ => SetNeedsDraw()
            )
        );
    }

    private async Task InitializeViewAsync()
    {
        try
        {
            await ViewModel.InitializeAsync();
            RefreshTextCache();
            SetNeedsDraw();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init Error: {ex.Message}");
        }
    }
}
