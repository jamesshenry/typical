namespace Typical.Core.Statistics;

public class GameStats
{
    private readonly TimeProvider _timeProvider;
    private readonly List<KeystrokeLog> _logs = [];

    // Running Totals (State)
    private int _correctCount;
    private int _incorrectCount;
    private int _extraCount;
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
            case KeystrokeType.Extra:
                _extraCount += change;
                break;
            case KeystrokeType.Correction:
                _correctionCount += change;
                break;
        }
    }

    internal void RecordKey(char c, KeystrokeType type)
    {
        if (!IsRunning)
            Start();

        UpdateCounts(type, 1);
        _logs.Add(new KeystrokeLog(c, type, _timeProvider.GetTimestamp()));
    }

    internal void RecordBackspace()
    {
        if (_logs.Count == 0)
            return;

        int indexToRemove = _logs.FindLastIndex(log => log.Type != KeystrokeType.Correction);

        if (indexToRemove != -1)
        {
            _logs.RemoveAt(indexToRemove);
        }
        _logs.Add(new KeystrokeLog('\b', KeystrokeType.Correction, _timeProvider.GetTimestamp()));
    }

    internal void Start() => _startTimestamp = _timeProvider.GetTimestamp();

    internal void Stop() => _endTimestamp = _timeProvider.GetTimestamp();

    public GameStatisticsSnapshot CreateSnapshot()
    {
        var elapsed = ElapsedTime;
        double wpm = elapsed.TotalMinutes > 0 ? _correctCount / 5.0 / elapsed.TotalMinutes : 0;

        int totalAttempted = _correctCount + _incorrectCount;
        double accuracy = totalAttempted > 0 ? _correctCount / (double)totalAttempted * 100 : 100;

        return new GameStatisticsSnapshot(
            WordsPerMinute: wpm,
            Accuracy: accuracy,
            Chars: new CharacterStats(
                _correctCount,
                _incorrectCount,
                _extraCount,
                _correctionCount
            ),
            ElapsedTime: elapsed,
            IsRunning: this.IsRunning
        );
    }

    public TimeSpan ElapsedTime =>
        _startTimestamp.HasValue
            ? _timeProvider.GetElapsedTime(
                _startTimestamp.Value,
                _endTimestamp ?? _timeProvider.GetTimestamp()
            )
            : TimeSpan.Zero;

    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;

    public IReadOnlyList<KeystrokeLog> GetHistory() => _logs.AsReadOnly();
}
