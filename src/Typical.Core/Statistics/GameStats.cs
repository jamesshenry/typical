namespace Typical.Core.Statistics;

public class GameStats(TimeProvider? timeProvider = null)
{
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private long? _startTimestamp;
    private long? _endTimestamp;
    private bool _statsAreDirty = true; // Start dirty
    private double _cachedWpm;
    private double _cachedAccuracy;
    private CharacterStats _cachedChars = new(0, 0, 0);
    public double WordsPerMinute
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return _cachedWpm;
        }
    }

    public double Accuracy
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return _cachedAccuracy;
        }
    }

    public CharacterStats Chars
    {
        get
        {
            if (_statsAreDirty)
                RecalculateAllStats();
            return _cachedChars;
        }
    }
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;
    public TimeSpan ElapsedTime =>
        _timeProvider.GetElapsedTime(
            _startTimestamp ?? 0,
            _endTimestamp ?? _timeProvider.GetTimestamp()
        );

    public void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
    }

    public void Reset()
    {
        _startTimestamp = null;
        _endTimestamp = null;
        _keystrokeHistory.Clear();
        _cachedWpm = 0;
        _cachedAccuracy = 100;
        _cachedChars = new CharacterStats(0, 0, 0);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
        }
    }

    private void RecalculateAllStats()
    {
        _cachedWpm = _keystrokeHistory.CalculateWpm(ElapsedTime);
        _cachedAccuracy = _keystrokeHistory.CalculateAccuracy();
        _cachedChars = _keystrokeHistory.GetCharacterStats();

        _statsAreDirty = false; // The stats are now fresh
    }

    internal void LogKeystroke(char keyChar, KeystrokeType extra)
    {
        if (!IsRunning)
        {
            Start();
        }
        _keystrokeHistory.Add(new KeystrokeLog(keyChar, extra, _timeProvider.GetTimestamp()));
        _statsAreDirty = true; // Mark stats as dirty
    }
}
