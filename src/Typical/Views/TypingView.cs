using System.ComponentModel;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace Typical.Views;

public class TypingArea : View
{
    private readonly Attribute _correctAttr;
    private readonly Attribute _incorrectAttr;
    private readonly Attribute _untypedAttr;
    private readonly TextFormatter _formatter = new();
    private List<string> _cachedLines = [];
    private readonly TypingViewModel _viewModel;

    public TypingArea(TypingViewModel viewModel)
    {
        _viewModel = viewModel;
        _formatter.WordWrap = true;

        var scheme = this.GetScheme();
        var normalBack = scheme.Normal.Background;

        _correctAttr = new Attribute(Color.Green, Color.DarkGray);
        _incorrectAttr = new Attribute(Color.White, Color.Red);
        _untypedAttr = new Attribute(Color.DarkGray, normalBack);
    }

    public void RefreshText()
    {
        if (Viewport.Width <= 0)
            return;

        _formatter.Text = _viewModel.TargetText;
        _formatter.ConstrainToWidth = Viewport.Width;
        _formatter.PreserveTrailingSpaces = true;
        _cachedLines = _formatter.GetLines();

        if (Height != _cachedLines.Count)
        {
            Height = _cachedLines.Count;
            SuperView?.SetNeedsLayout();
        }
        SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (_cachedLines.Count == 0 || Viewport.Width == 0)
            return true;

        int yOffset = Math.Max(0, (Viewport.Height - _cachedLines.Count) / 2);
        int globalIdx = 0;
        for (int y = 0; y < _cachedLines.Count; y++)
        {
            string line = _cachedLines[y];
            int xOffset = Math.Max(0, (Viewport.Width - line.Length) / 2);

            for (int x = 0; x < line.Length; x++)
            {
                if (globalIdx >= _viewModel.DisplayStates.Length)
                    break;

                var state = _viewModel.DisplayStates[globalIdx];

                SetAttribute(GetAttributeForState(state));
                AddRune(x + xOffset, y + yOffset, (Rune)line[x]);

                globalIdx++;
            }
        }
        return true;
    }

    private Attribute GetAttributeForState(KeystrokeType state) =>
        state switch
        {
            KeystrokeType.Correct => _correctAttr,
            KeystrokeType.Incorrect => _incorrectAttr,
            _ => _untypedAttr,
        };
}

public class TypingView : BindableView<TypingViewModel>
{
    private readonly TypingArea _typingArea;

    public TypingView(TypingViewModel viewModel)
        : base(viewModel)
    {
        CanFocus = true;
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Fill();
        Height = Dim.Fill();

        _typingArea = new TypingArea(viewModel)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        Add(_typingArea);

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
        _typingArea.RefreshText();
        _typingArea.SetNeedsDraw();
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
                _typingArea.RefreshText();
                SetNeedsLayout();
            }
            _typingArea.SetNeedsDraw();
        });
    }

    protected override void SetupBindings() { }

    private async Task InitializeViewAsync()
    {
        try
        {
            await ViewModel.InitializeAsync();
            _typingArea.RefreshText();
            SetNeedsDraw();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init Error: {ex.Message}");
        }
    }
}
