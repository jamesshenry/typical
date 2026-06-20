using System.Drawing;
using Stanza.TerminalGui;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<ResultsViewModel>]
public partial class ResultsDialog : Dialog
{
    private readonly GraphView _graph;
    private readonly Label _sourceLabel;

    protected override bool OnAccepting(CommandEventArgs args)
    {
        if (base.OnAccepting(args))
        {
            return true;
        }
        return false;
    }

    public ResultsDialog(ResultsViewModel viewModel)
    {
        Height = Dim.Percent(90);
        Width = Dim.Percent(50);
        _graph = new GraphView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(), // leave room for buttons
            MarginLeft = 3, // Space for Y-axis labels (e.g. "120.00")
            MarginBottom = 2, // Space for X-axis labels
        };

        _graph.CellSize = new PointF(2.0f, 5.0f);

        //_graph.MarginLeft = 3;
        //_graph.MarginBottom = 2;

        // X Axis (Time)
        _graph.AxisX.Text = "Time (s)";
        _graph.AxisX.Increment = 5f; // Tick every 5 seconds
        _graph.AxisX.ShowLabelsEvery = 2; // Label every 10 seconds (5 * 2)
        _graph.AxisX.LabelGetter = (v) => $"{v.Value:0}s";

        // Y Axis (WPM)
        _graph.AxisY.Text = "WPM";
        _graph.AxisY.Increment = 5f; // Tick every 10 WPM
        _graph.AxisY.ShowLabelsEvery = 2; // Label every tick (10, 20, 30...)
        Add(_graph);

        _sourceLabel = new Label();
        _sourceLabel.Text = "Unknown";
        _sourceLabel.Y = Pos.Bottom(_graph);
        Add(_sourceLabel);
        AddButton(new() { Text = "_Ok", X = Pos.Center() });
        ViewModel = viewModel;
    }

    partial void OnApplyBindings(BindingContext context)
    {
        if (ViewModel is null)
            return;

        var ctx = ViewModel.GraphContext;

        var data = ctx.Points;

        if (!data.Any())
            return;

        // 1. Get the "Perfect Scale" from our data object
        var (cellSize, scrollOffset, yIncrement) = ctx.GetScale(
            _graph.Viewport.Size,
            _graph.MarginLeft,
            _graph.MarginBottom
        );

        // 2. Apply to UI
        _graph.CellSize = cellSize;
        _graph.ScrollOffset = scrollOffset;
        _graph.AxisY.Minimum = scrollOffset.Y;
        _graph.AxisY.Increment = yIncrement;

        _graph.Series.Clear();
        _graph.Series.Add(new ScatterSeries { Points = data });

        _graph.Annotations.Clear();
        var path = new PathAnnotation { BeforeSeries = true };
        path.Points.AddRange(data);
        _graph.Annotations.Add(path);

        _graph.SetNeedsDraw();

        _sourceLabel.Text = ViewModel.Source;
        SetNeedsDraw();
    }
}
