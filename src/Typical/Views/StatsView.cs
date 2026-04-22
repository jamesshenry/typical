using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.Views;

public class StatsView : BindableView<StatsViewModel>
{
    private readonly Label _statsLabel;

    public StatsView(StatsViewModel viewModel)
        : base(viewModel)
    {
        Title = nameof(StatsView);
        BorderStyle = LineStyle.None;
        Height = 3;
        Width = Dim.Fill();
        _statsLabel = new Label { X = Pos.Center(), Y = Pos.Center() };
        Add(_statsLabel);
    }

    protected override void SetupBindings()
    {
        Bind(
            () => ViewModel.Stats,
            stats =>
            {
                if (stats is null)
                    return;
                _statsLabel.Text =
                    $"Elapsed: {stats.ElapsedTime:mm\\:ss} WPM: {Math.Round(stats.WordsPerMinute)} | Acc: {stats.Accuracy.ToString()}";
                SetNeedsDraw();
            }
        );
    }
}
