using Stanza.TerminalGui;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<StatsViewModel>]
public partial class StatsView : View
{
    [BindText(nameof(StatsViewModel.StatsLabel))]
    private readonly Label _statsLabel;

    public StatsView(StatsViewModel viewModel)
    {
        Title = nameof(StatsView);
        BorderStyle = LineStyle.None;
        Height = 3;
        Width = Dim.Fill();
        _bindingContext = new BindingContext();
        _statsLabel = new Label { X = Pos.Center(), Y = Pos.Center() };
        Add(_statsLabel);

        ViewModel = viewModel;
    }

    partial void OnApplyBindings(BindingContext context) { }
}
