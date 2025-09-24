using Typical.Core.Events;

namespace Typical.Core.Statistics;

internal class GameStats
{
    private readonly IEventAggregator _eventAggregator;
    private readonly TimeProvider _timeProvider;
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private long? _startTimestamp;
    private long? _endTimestamp;
    private bool _statsAreDirty = true;
    private double _cachedWpm;
    private double _cachedAccuracy;
    private CharacterStats _cachedChars = new(0, 0, 0, 0);

    public GameStats(IEventAggregator eventAggregator, TimeProvider? timeProvider = null)
    {
        _eventAggregator = eventAggregator;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _eventAggregator.Subscribe<KeyPressedEvent>(OnKeyPressed);
        _eventAggregator.Subscribe<BackspacePressedEvent>(OnBackspacePressed);
    }

    private void OnBackspacePressed(BackspacePressedEvent @event)
    {
        if (!IsRunning)
        {
            return;
        }

        _keystrokeHistory.RemoveLastCharacterLog();
        _keystrokeHistory.Add(
            new KeystrokeLog('\b', KeystrokeType.Correction, _timeProvider.GetTimestamp())
        );

        _statsAreDirty = true;
    }

    private void OnKeyPressed(KeyPressedEvent @event)
    {
        if (!IsRunning)
        {
            Start();
        }

        _keystrokeHistory.Add(
            new KeystrokeLog(@event.Character, @event.Type, _timeProvider.GetTimestamp())
        );
        _statsAreDirty = true;
    }

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
        _cachedChars = new CharacterStats(0, 0, 0, 0);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
        }
    }

    public GameStatisticsSnapshot CreateSnapshot()
    {
        if (_statsAreDirty)
        {
            RecalculateAllStats();
        }

        return new GameStatisticsSnapshot(
            WordsPerMinute: _cachedWpm,
            Accuracy: _cachedAccuracy,
            Chars: _cachedChars,
            ElapsedTime: this.ElapsedTime,
            IsRunning: this.IsRunning
        );
    }

    private void RecalculateAllStats()
    {
        _cachedWpm = _keystrokeHistory.CalculateWpm(ElapsedTime);
        _cachedAccuracy = _keystrokeHistory.CalculateAccuracy();
        _cachedChars = _keystrokeHistory.GetCharacterStats();

        _statsAreDirty = false;
    }
}
