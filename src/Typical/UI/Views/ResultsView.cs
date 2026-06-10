using Stanza.TerminalGui;
using Terminal.Gui.Input;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<ResultsViewModel>]
public partial class ResultsDialog : Dialog
{
    protected override bool OnAccepting(CommandEventArgs args)
    {
        if (base.OnAccepting(args))
        {
            return true;
        }
        return false;
    }

    public ResultsDialog(ResultsViewModel viewModel)
    {
        AddButton(new() { Text = "_Cancel" });
        AddButton(new() { Text = "_Ok" });
        Add(new CheckBox());
        ViewModel = viewModel;
    }
}
