using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class SettingsView : BindableView<SettingsViewModel>
{
    private readonly TextField _txtName;
    private readonly CheckBox _chkLog;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public SettingsView(SettingsViewModel viewModel)
        : base(viewModel)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        var lblName = new Label { Text = "Username:" };
        _txtName = new TextField { X = Pos.Right(lblName) + 2, Width = Dim.Fill(5) };

        _chkLog = new CheckBox { Y = Pos.Bottom(lblName) + 1, Text = "Enable Background Logging" };

        _btnSave = new Button
        {
            X = 0,
            Y = Pos.Bottom(_chkLog) + 2,
            Text = "Save Settings",
        };

        _btnCancel = new Button
        {
            X = Pos.Right(_btnSave) + 2,
            Y = Pos.Y(_btnSave),
            Text = "Cancel",
        };

        Add(lblName, _txtName, _chkLog, _btnSave, _btnCancel);
    }

    protected override void SetupBindings()
    {
        BindingContext.AddBinding(
            ViewModel.BindText(
                _txtName,
                () => ViewModel.Username,
                value => ViewModel.Username = value
            )
        );

        BindingContext.AddBinding(
            ViewModel.BindChecked(
                _chkLog,
                () => ViewModel.EnableLogging,
                value => ViewModel.EnableLogging = value
            )
        );

        BindingContext.AddBinding(ViewModel.BindCommand(ViewModel.SaveCommand, _btnSave));
        BindingContext.AddBinding(ViewModel.BindCommand(ViewModel.CancelCommand, _btnCancel));
    }
}
