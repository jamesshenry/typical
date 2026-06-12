using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Typical.Core.Interfaces;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public class ResultsViewModel : ObservableObject, IModalViewModel<bool>
{
    public bool Result => true;

    public ObservableCollection<TestSnapshot> Snapshots { get; } = new();

    public event EventHandler? RequestClose;

    public void Initialize(TestResult result)
    {
        Snapshots.Clear();
        foreach (var snap in result.Snapshots)
        {
            Snapshots.Add(snap);
        }
    }
}
