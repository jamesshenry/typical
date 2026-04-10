using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class HomeView : BindableView<HomeViewModel>
{
    public HomeView(HomeViewModel vm)
        : base(vm)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        var btn = new Button { X = Pos.Center(), Text = "Go Settings" };

        btn.Accepting += (s, e) => ViewModel.NavigateSettingsCommand.Execute(null);
        Add(btn);
    }

    protected override void SetupBindings() { }
}
