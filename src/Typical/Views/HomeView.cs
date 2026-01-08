using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class HomeView : BindableView<HomeViewModel>
{
    private readonly Label _lbl;

    public HomeView(HomeViewModel vm)
        : base(vm)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        _lbl = new Label { X = Pos.Center(), Y = Pos.Center() };

        var btn = new Button
        {
            X = Pos.Center(),
            Y = Pos.Bottom(_lbl),
            Text = "Go Settings",
        };

        btn.Accepting += (s, e) => ViewModel.NavigateSettingsCommand.Execute(null);
        Add(_lbl);
        Add(btn);
    }

    protected override void SetupBindings()
    {
        BindingContext.AddBinding(
            _lbl.BindTextOneWay(
                ViewModel,
                () => ViewModel.WelcomeMessage,
                nameof(ViewModel.WelcomeMessage)
            )
        );
    }
}
