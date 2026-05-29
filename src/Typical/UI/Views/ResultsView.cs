using Terminal.Gui.Input;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

public class ResultsDialog : TypicalDialog<ResultsViewModel>
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
        : base(viewModel)
    {
        AddButton(new() { Text = "_Cancel" });
        AddButton(new() { Text = "_Ok" });
    }
}
