using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;

namespace Typical.UI.Views;

public class ResultsView : BindableView<ResultsViewModel>
{
    public ResultsView(ResultsViewModel viewModel)
        : base(viewModel)
    {
        var text = new TextField() { Text = "Results View" };
        text.X = Pos.Center();
        text.Y = Pos.Center();
        text.Width = Dim.Fill();
        text.Height = Dim.Fill();

        Add(text);
    }
}
