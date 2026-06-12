using System.Drawing;

using Stanza.TerminalGui;

using Terminal.Gui.Input;
using Terminal.Gui.Views;

using Typical.Core.ViewModels;

namespace Typical.UI.Views;

[StanzaView<ResultsViewModel>]
public partial class ResultsDialog : Dialog
{
    private readonly GraphView _graph;

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
        AddButton(new() { Text = "_Cancel" });
        AddButton(new() { Text = "_Ok" });
        Add(new CheckBox());
        ViewModel = viewModel;

        _graph = new GraphView();
    }
    partial void OnApplyBindings(BindingContext context)
    {
        if (ViewModel == null) return;

        Action refreshGraph = () =>
        {
            _graph.Reset();

            var wpmPoints = ViewModel.Snapshots
                .Select(s => new PointF((float)s.ElapsedTime.TotalSeconds, (float)s.WPM.Value))
                .ToList();

            _graph.Series.Add(new ScatterSeries
            {
                Points = wpmPoints,
            });
            _graph.SetNeedsDraw();
        };
    }
}
