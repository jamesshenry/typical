using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class SettingsView : BindableView<SettingsViewModel>
{
    private readonly Button _btnQuoteMode;

    public SettingsView(SettingsViewModel viewModel)
        : base(viewModel)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        _btnQuoteMode = new Button { X = Pos.Center(), Text = "Quote" };

        Add(_btnQuoteMode);
    }

    protected override void SetupBindings()
    {
        BindingContext.AddBinding(ViewModel.BindCommand(ViewModel.QuoteModeCommand, _btnQuoteMode));
    }
}
