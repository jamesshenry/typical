using System.Collections.ObjectModel;
using System.Drawing;

using CommunityToolkit.Mvvm.ComponentModel;

using Typical.Core.Interfaces;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public class ResultsViewModel : ObservableObject, IModalViewModel<bool>
{
    public bool Result => true;

    public ObservableCollection<TestSnapshot> Snapshots { get; } = new();
    public IEnumerable<PointF> WpmSeriesData =>
        Snapshots.Select(s => new PointF((float)s.ElapsedTime.TotalSeconds, (float)s.WPM.Value));
    public GraphData GraphContext => new GraphData(Snapshots);

    public string Source { get; private set; } = string.Empty;

    public event EventHandler? RequestClose;

    public void Initialize(TestResult result)
    {
        Source = result.Target.Source;
        Snapshots.Clear();
        foreach (var snap in result.Snapshots)
        {
            Snapshots.Add(snap);
        }
    }
}
public class GraphData
{
    private readonly List<TestSnapshot> _snapshots = new List<TestSnapshot>();

    public GraphData(IEnumerable<TestSnapshot> snapshots)
    {
        _snapshots = snapshots.ToList();
    }

    public List<PointF> Points => _snapshots
        .Select(s => new PointF((float)s.ElapsedTime.TotalSeconds, (float)s.WPM)).ToList();

    // ANALYZE: Provides the bounds for scaling
    public float MinWpm => _snapshots.Any() ? _snapshots.Min(s => (float)s.WPM) : 0;
    public float MaxWpm => _snapshots.Any() ? _snapshots.Max(s => (float)s.WPM) : 100;
    public float TotalSeconds => _snapshots.Any() ? (float)_snapshots.Max(s => s.ElapsedTime.TotalSeconds) : 0;

    // CALCULATE: Logic for the "Perfect Frame"
    public (PointF CellSize, PointF ScrollOffset, float Increment) GetScale(Size viewport, uint marginLeft, uint marginBottom)
    {
        float availableWidth = viewport.Width - marginLeft;
        float availableHeight = viewport.Height - marginBottom;

        float buffer = 10f;
        float yMin = Math.Max(0, MinWpm - buffer);
        float yRange = (MaxWpm + buffer) - yMin;

        // X and Y scaling
        float cx = availableWidth > 0 ? TotalSeconds / availableWidth : 1f;
        float cy = availableHeight > 0 ? yRange / availableHeight : 1f;

        // Axis Ticks: One label every 4 rows is a good visual density
        float increment = Math.Max(10f, (float)Math.Ceiling(cy * 4 / 5.0) * 5);

        return (new PointF(cx, cy), new PointF(0, yMin), increment);
    }

}
