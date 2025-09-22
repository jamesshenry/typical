using System.Diagnostics;

namespace Typical.Core;

public class GameStats(TimeProvider? timeProvider = null)
{
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private long? _startTimestamp;
    private long? _endTimestamp;

    public double WordsPerMinute { get; private set; }
    public double Accuracy { get; private set; }
    public CharacterStats Chars { get; private set; } = new(0, 0, 0);
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
        WordsPerMinute = 0;
        Accuracy = 100;
        Chars = new CharacterStats(0, 0, 0);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
        }
    }

    public void CalculateStats()
    {
        WordsPerMinute = _keystrokeHistory.CalculateWpm(ElapsedTime);
        Accuracy = _keystrokeHistory.CalculateAccuracy();
        Chars = _keystrokeHistory.GetCharacterStats();
    }

    internal void LogKeystroke(char keyChar, KeystrokeType extra)
    {
        if (!IsRunning)
        {
            Start();
        }
        _keystrokeHistory.Add(new KeystrokeLog(keyChar, extra, _timeProvider.GetTimestamp()));
    }
}
