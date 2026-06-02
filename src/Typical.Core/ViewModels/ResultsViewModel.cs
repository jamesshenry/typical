using CommunityToolkit.Mvvm.ComponentModel;

using Typical.Core.Interfaces;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public class ResultsViewModel : ObservableObject, IModalViewModel<bool>
{
    public bool Result => true;

    public event EventHandler? RequestClose;

    public void Initialize(TestResult result)
    {
        // TODO: finish implementing display of results
    }
}
