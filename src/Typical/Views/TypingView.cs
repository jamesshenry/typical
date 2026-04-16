using System.ComponentModel;
using System.Text;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Typical.Core.ViewModels;

namespace Typical.Views;

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
