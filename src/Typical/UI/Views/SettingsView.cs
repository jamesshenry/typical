using Stanza.TerminalGui;

using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<SettingsViewModel>]
public partial class SettingsView : View
{
    [BindCommand(nameof(SettingsViewModel.QuoteModeCommand))]
    private readonly Button _btnQuoteMode;

    public SettingsView(SettingsViewModel viewModel)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        _btnQuoteMode = new Button { X = Pos.Center(), Text = "Quote" };

        Add(_btnQuoteMode);

        ViewModel = viewModel;
    }
}
