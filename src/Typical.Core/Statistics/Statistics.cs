using Typical.Core.Text;

namespace Typical.Core.Statistics;

public class Statistics
{
    private readonly TimeProvider _timeProvider;
    private readonly KeystrokeCollection _keystrokes = new();
    private readonly List<TestSnapshot> _snapshots = [];
    private long? _startTimestamp;
    private long? _endTimestamp;

    public Statistics(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public IReadOnlyList<KeystrokeLog> Keystrokes => _keystrokes.GetLog();

    public IReadOnlyList<TestSnapshot> Snapshots => _snapshots.AsReadOnly();
    public TimeSpan ElapsedTime =>
        _startTimestamp.HasValue
            ? _timeProvider.GetElapsedTime(
                _startTimestamp.Value,
                _endTimestamp ?? _timeProvider.GetTimestamp()
            )
            : TimeSpan.Zero;
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;

    internal void RecordKey(string grapheme, KeystrokeType type, int currentIndex)
    {
        if (!IsRunning)
            Start();

        _keystrokes.Add(grapheme, type, _timeProvider.GetTimestamp(), currentIndex);
    }

    internal void RecordBackspace(int currentIndex)
    {
        _keystrokes.Add("\b", KeystrokeType.Correction, _timeProvider.GetTimestamp(), currentIndex);
    }

    internal void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
    }

    private void Reset()
    {
        _startTimestamp = null;
        _endTimestamp = null;
        _keystrokes.Clear();
        _snapshots.Clear();
    }

    internal void Stop() => _endTimestamp = _timeProvider.GetTimestamp();

    public TestSnapshot CreateSnapshot()
    {
        var characterStats = new CharacterStats(
            _keystrokes.CorrectCount,
            _keystrokes.ErrorCount,
            _keystrokes.CorrectionCount
        );
        var snapshot = TestSnapshot.Create(characterStats, ElapsedTime);

        _snapshots.Add(snapshot);

        return snapshot;
    }

    internal TestResult GetFinalResult(TextSample targetSample)
    {
        var finalSnapshot = CreateSnapshot();
        double minutes = ElapsedTime.Minutes;
        double rawWpm = (minutes <= 0) ? 0 : (_keystrokes.TotalPhysical / 5.0) / minutes;
        return new TestResult(
            PlayedAt: DateTime.UtcNow,
            FinalWpm: finalSnapshot.WPM,
            RawWpm: WPM.From(rawWpm),
            FinalAccuracy: finalSnapshot.Accuracy,
            Duration: finalSnapshot.ElapsedTime,
            Target: targetSample,
            Telemetry: Keystrokes.ToList(),
            Snapshots: Snapshots.ToList()
        );
    }
}
