using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stanza.TerminalGui;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<TypingViewModel>]
public partial class TypingView : View
{
    private readonly TypingArea _typingArea;
    private readonly Label _sourceLabel;
    private readonly ILogger<TypingView> _logger;

    public TypingView(TypingViewModel viewModel, ILogger<TypingView>? logger = null)
    {
        CanFocus = true;
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Fill();
        Height = Dim.Fill();
        ViewModel = viewModel;
        _logger = logger ?? NullLogger<TypingView>.Instance;
        _bindingContext = new BindingContext();
        _typingArea = new TypingArea(viewModel)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        _sourceLabel = new Label();
        Add(_typingArea);

        this.Bind(
                ViewModel,
                vm => vm.Target,
                target =>
                {
                    _typingArea.Refresh();
                    _sourceLabel.Text = target?.Source ?? string.Empty;
                }
            )
            .AddTo(_bindingContext);

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
                ViewModel?.ProcessInput(key.AsGrapheme, isBackspace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Input Error: {ex.Message}");
            }

            return true;
        }

        return false;
    }

    private async Task InitializeViewAsync()
    {
        try
        {
            await (ViewModel?.InitializeAsync() ?? Task.CompletedTask);
            _typingArea.Refresh();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Init Error: {ex.Message}");
        }
    }
}
