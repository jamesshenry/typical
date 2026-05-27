using System.ComponentModel;
using System.Text;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

public class TypingView : BindableView<TypingViewModel>
{
    private readonly TypingArea _typingArea;
    private readonly Label _sourceLabel;

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
        _sourceLabel = new Label();
        Add(_typingArea);

        ViewModel.RefreshRequested += OnViewModelRefreshRequested;

        Initialized += (s, e) =>
        {
            _ = InitializeViewAsync();
        };
        this.Activating += (s, e) =>
        {
            this.SetFocus();
            e.Handled = true; // Prevents the click from reaching MainShell
        };
    }

    private void OnViewModelRefreshRequested(object? sender, EventArgs e)
    {
        App?.Invoke(() => ViewModel.RefreshState());
    }

    protected override void OnSubViewsLaidOut(LayoutEventArgs args)
    {
        base.OnSubViewsLaidOut(args);
        _typingArea.Refresh();
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

        if (rune == default)
            return base.OnKeyDown(key);

        if (rune != default || isBackspace)
        {
            try
            {
                ViewModel.ProcessInput(key.AsGrapheme, isBackspace);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Input Error: {ex.Message}");
            }

            return true;
        }

        return false;
    }

    protected override void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        App?.Invoke(() =>
        {
            if (e.PropertyName == nameof(ViewModel.Target))
            {
                _typingArea.Refresh();
                _sourceLabel.Text = ViewModel.Target.Source;
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ViewModel.RefreshRequested -= OnViewModelRefreshRequested;
        }

        base.Dispose(disposing);
    }

    private async Task InitializeViewAsync()
    {
        try
        {
            await ViewModel.InitializeAsync();
            _typingArea.Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init Error: {ex.Message}");
        }
    }
}
