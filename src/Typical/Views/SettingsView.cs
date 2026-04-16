using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class SettingsView : BindableView<SettingsViewModel>
{
    // private readonly CheckBox _chkLog;
    private readonly Button _btnQuoteMode;

    // private readonly Button _btnCancel;

    public SettingsView(SettingsViewModel viewModel)
        : base(viewModel)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        // _chkLog = new CheckBox { X = Pos.Right(_txtName) + 1, Text = "Enable Background Logging" };

        _btnQuoteMode = new Button { X = Pos.Center(), Text = "Quote" };

        // _btnCancel = new Button { X = Pos.Right(_btnQuoteMode) + 1, Text = "Cancel" };

        Add(_btnQuoteMode);
    }

    protected override void SetupBindings()
    {
        // BindingContext.AddBinding(
        //     ViewModel.BindText(
        //         _txtName,
        //         () => ViewModel.Username,
        //         value => ViewModel.Username = value
        //     )
        // );

        // BindingContext.AddBinding(
        //     ViewModel.BindChecked(
        //         _chkLog,
        //         () => ViewModel.EnableLogging,
        //         value => ViewModel.EnableLogging = value
        //     )
        // );

        BindingContext.AddBinding(ViewModel.BindCommand(ViewModel.QuoteModeCommand, _btnQuoteMode));
    }
}
