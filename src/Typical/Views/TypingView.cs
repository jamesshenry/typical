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
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace Typical.Views;

public class TypingView : BindableView<TypingViewModel>
{
    private readonly TextFormatter _formatter = new();
    private List<string> _cachedLines = [];
    private readonly Attribute _correctAttr;
    private readonly Attribute _incorrectAttr;
    private readonly Attribute _untypedAttr;

    public TypingView(TypingViewModel viewModel)
        : base(viewModel)
    {
        CanFocus = true;
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Percent(80);
        Height = Dim.Percent(50);
        Title = nameof(TypingView);
        _formatter.WordWrap = true;

        Initialized += (s, e) => _ = InitializeViewAsync();
        this.Activating += (s, e) =>
        {
            this.SetFocus();
            e.Handled = true; // Prevents the click from reaching MainShell
        };

        var scheme = this.GetScheme();
        var normalBack = scheme.Normal.Background;

        _correctAttr = new Attribute(Color.Green, Color.DarkGray);
        _incorrectAttr = new Attribute(Color.White, Color.Red);
        _untypedAttr = new Attribute(Color.DarkGray, normalBack);
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
        _formatter.PreserveTrailingSpaces = true;
        _cachedLines = _formatter.GetLines();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (_cachedLines.Count == 0)
            return true;

        int globalIdx = 0;
        for (int y = 0; y < _cachedLines.Count; y++)
        {
            for (int x = 0; x < _cachedLines[y].Length; x++)
            {
                var state = ViewModel.DisplayStates[globalIdx];

                SetAttribute(GetAttributeForState(state));
                AddRune(x, y, (Rune)_cachedLines[y][x]);

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

    protected override void SetupBindings() { }

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
