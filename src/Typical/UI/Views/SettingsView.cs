using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;
using Stanza.TerminalGui;

namespace Typical.UI.Views;

public class SettingsView : View
{
    private readonly Button _btnQuoteMode;
    private readonly BindingContext _bindingContext;

    public SettingsView(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        _bindingContext = new BindingContext();
        Width = Dim.Fill();
        Height = Dim.Fill();

        _btnQuoteMode = new Button { X = Pos.Center(), Text = "Quote" };

        Add(_btnQuoteMode);

        ViewModel.QuoteModeCommand.BindCommand(_btnQuoteMode).AddTo(_bindingContext);
    }

    public SettingsViewModel ViewModel { get; }

   protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bindingContext.Dispose(); // Cleans up command event subscriptions safely [1]
        }
        base.Dispose(disposing);
    }

    // protected override void SetupBindings()
    // {
    //     BindingContext.AddBinding(ViewModel.BindCommand(ViewModel.QuoteModeCommand, _btnQuoteMode));
    // }
}
