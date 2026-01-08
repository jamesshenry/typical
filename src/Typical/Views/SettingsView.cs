using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class SettingsView : BindableView<SettingsViewModel>
{
    private readonly TextField _txtName;
    private readonly CheckBox _chkLog;

    public SettingsView(SettingsViewModel viewModel)
        : base(viewModel)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        var lblName = new Label { Text = "Username:" };
        _txtName = new TextField { X = Pos.Right(lblName) + 2, Width = Dim.Fill(5) };

        _chkLog = new CheckBox { Y = Pos.Bottom(lblName) + 1, Text = "Enable Background Logging" };

        var btnSave = new Button
        {
            X = 0,
            Y = Pos.Bottom(_chkLog) + 2,
            Text = "Save Settings",
        };

        var btnCancel = new Button
        {
            X = Pos.Right(btnSave) + 2,
            Y = Pos.Y(btnSave),
            Text = "Cancel",
        };

        btnSave.Accepting += (s, e) => ViewModel.SaveCommand.Execute(null);
        btnCancel.Accepting += (s, e) => ViewModel.CancelCommand.Execute(null);

        Add(lblName, _txtName, _chkLog, btnSave, btnCancel);
    }

    protected override void SetupBindings()
    {
        BindingContext.AddBinding(
            _txtName.BindTextTwoWay(
                ViewModel,
                () => ViewModel.Username,
                value => ViewModel.Username = value,
                nameof(ViewModel.Username)
            )
        );

        BindingContext.AddBinding(
            _chkLog.BindCheckedTwoWay(
                ViewModel,
                () => ViewModel.EnableLogging,
                value => ViewModel.EnableLogging = value,
                nameof(ViewModel.EnableLogging)
            )
        );
    }
}
