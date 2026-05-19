namespace Typical.Core.Statistics;

public class GameStats
{
    private readonly TimeProvider _timeProvider;
    private readonly KeystrokeCollection _keystrokes = new();
    private readonly List<GameStatsSnapshot> _snapshots = [];
    private long? _startTimestamp;
    private long? _endTimestamp;

    public GameStats(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyList<KeystrokeLog> Keystrokes => _keystrokes.GetLog();

    public IReadOnlyList<GameStatsSnapshot> Snapshots => _snapshots.AsReadOnly();
    public TimeSpan ElapsedTime =>
        _startTimestamp.HasValue
            ? _timeProvider.GetElapsedTime(
                _startTimestamp.Value,
                _endTimestamp ?? _timeProvider.GetTimestamp()
            )
            : TimeSpan.Zero;
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;

    internal void RecordKey(string grapheme, KeystrokeType type)
    {
        if (!IsRunning)
            Start();

        _keystrokes.Add(grapheme, type, _timeProvider.GetTimestamp());
    }

    internal void RecordBackspace()
    {
        _keystrokes.Add("\b", KeystrokeType.Correction, _timeProvider.GetTimestamp());
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

    public GameStatsSnapshot CreateSnapshot()
    {
        var characterStats = new CharacterStats(
            _keystrokes.CorrectCount,
            _keystrokes.ErrorCount,
            _keystrokes.CorrectionCount
        );
        var snapshot = GameStatsSnapshot.Create(characterStats, ElapsedTime);

        _snapshots.Add(snapshot);

        return snapshot;
    }
}
