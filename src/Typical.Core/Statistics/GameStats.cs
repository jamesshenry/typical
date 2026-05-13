using System.Diagnostics;

namespace Typical.Core.Statistics;

public class GameStats
{
    private readonly TimeProvider _timeProvider;
    private readonly List<KeystrokeLog> _keystrokes = [];
    private readonly List<GameSnapshot> _snapshots = [];
    public IReadOnlyList<KeystrokeLog> Keystrokes => _keystrokes.AsReadOnly();
    public IReadOnlyList<GameSnapshot> Snapshots => _snapshots.AsReadOnly();

    // Running Totals (State)
    private int _correctCount;
    private int _incorrectCount;
    private int _correctionCount;
    private long? _startTimestamp;
    private long? _endTimestamp;

    public GameStats(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private void UpdateCounts(KeystrokeType type, int change)
    {
        switch (type)
        {
            case KeystrokeType.Correct:
                _correctCount += change;
                break;
            case KeystrokeType.Incorrect:
                _incorrectCount += change;
                break;
            case KeystrokeType.Correction:
                _correctionCount += change;
                break;
        }
    }

    internal void RecordKey(string grapheme, KeystrokeType type)
    {
        if (!IsRunning)
            Start();

        UpdateCounts(type, 1);
        _keystrokes.AddAndDebug(new KeystrokeLog(grapheme, type, _timeProvider.GetTimestamp()));
    }

    internal void RecordBackspace()
    {
        UpdateCounts(KeystrokeType.Correction, 1);
        _keystrokes.AddAndDebug(
            new KeystrokeLog("\b", KeystrokeType.Correction, _timeProvider.GetTimestamp())
        );
    }

    internal void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
    }

    private void Reset()
    {
        _endTimestamp = null;
        _keystrokes.Clear();
        _snapshots.Clear();
        _correctCount = 0;
        _correctionCount = 0;
        _incorrectCount = 0;
        _correctCount = 0;
    }

    internal void Stop() => _endTimestamp = _timeProvider.GetTimestamp();

    public GameSnapshot CreateSnapshot(string targetText, string userInput)
    {
        var elapsed = ElapsedTime;

        var snapshot = GameSnapshot.Create(
            _correctCount,
            _correctCount + _incorrectCount + _correctionCount,
            _incorrectCount,
            elapsed,
            targetText,
            userInput
        );

        _snapshots.Add(snapshot);

        return snapshot;
    }

    public TimeSpan ElapsedTime =>
        _startTimestamp.HasValue
            ? _timeProvider.GetElapsedTime(
                _startTimestamp.Value,
                _endTimestamp ?? _timeProvider.GetTimestamp()
            )
            : TimeSpan.Zero;

    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;
}

public static class ListExtensions
{
    extension(List<KeystrokeLog> logs)
    {
        public void AddAndDebug(KeystrokeLog log)
        {
            logs.Add(log);
            Debug.WriteLine(log);
        }
    }
}
