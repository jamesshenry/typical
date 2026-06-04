using Stanza.TerminalGui;

using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

using Typical.Core.ViewModels;

namespace Typical.UI.Views;

public class StatsView : View
{
    private readonly Label _statsLabel;
    private readonly BindingContext _bindingContext;
    public StatsView(StatsViewModel viewModel)
    {
        ViewModel = viewModel;
        Title = nameof(StatsView);
        BorderStyle = LineStyle.None;
        Height = 3;
        Width = Dim.Fill();
        _bindingContext = new BindingContext();
        _statsLabel = new Label { X = Pos.Center(), Y = Pos.Center() };
        Add(_statsLabel);

        ViewModel.Bind(this, vm => vm.Stats, stats =>
        {
            _statsLabel.Text =
    $"Elapsed: {stats.ElapsedTime:mm\\:ss} | WPM: {Math.Round(stats.WPM.Value)} | Acc: {stats.Accuracy.ToString()}";
            SetNeedsDraw();
        }).AddTo(_bindingContext);
    }

    public StatsViewModel ViewModel { get; }

    // protected override void SetupBindings()
    // {
    //     Bind(
    //         () => ViewModel.Stats,
    //         stats =>
    //         {
    //             _statsLabel.Text =
    //                 $"Elapsed: {stats.ElapsedTime:mm\\:ss} | WPM: {Math.Round(stats.WPM.Value)} | Acc: {stats.Accuracy.ToString()}";
    //             SetNeedsDraw();
    //         }
    //     );
    // }
}
