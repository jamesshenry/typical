using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Logging;

namespace Typical.Core.Statistics;

public class GameStats
{
    private readonly IEventAggregator _eventAggregator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GameStats> _logger;
    private readonly KeystrokeHistory _keystrokeHistory = [];
    private long? _startTimestamp;
    private long? _endTimestamp;
    private bool _statsAreDirty = true;
    private double _cachedWpm;
    private double _cachedAccuracy;
    private CharacterStats _cachedChars = new(0, 0, 0, 0);

    public GameStats(
        IEventAggregator eventAggregator,
        TimeProvider? timeProvider,
        ILogger<GameStats> logger
    )
    {
        _eventAggregator = eventAggregator;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger;
        _eventAggregator.Subscribe<KeyPressedEvent>(OnKeyPressed);
        _eventAggregator.Subscribe<BackspacePressedEvent>(OnBackspacePressed);
    }

    private void OnBackspacePressed(BackspacePressedEvent @event)
    {
        if (!IsRunning)
            return;

        CoreLogs.StatsBackspaceLogged(_logger);
        _keystrokeHistory.RemoveLastCharacterLog();
        _keystrokeHistory.Add(
            new KeystrokeLog('\b', KeystrokeType.Correction, _timeProvider.GetTimestamp())
        );
        _statsAreDirty = true;
    }

    private void OnKeyPressed(KeyPressedEvent @event)
    {
        if (!IsRunning)
            Start();

        CoreLogs.StatsKeyLogged(_logger, @event.Character, @event.Type);
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
        _timeProvider.GetElapsedTime(_startTimestamp ?? 0, _timeProvider.GetTimestamp());

    public void Start()
    {
        Reset();
        _startTimestamp = _timeProvider.GetTimestamp();
        CoreLogs.StatsStarted(_logger);
    }

    public void Reset()
    {
        _startTimestamp = null;
        _endTimestamp = null;
        _keystrokeHistory.Clear();
        _cachedWpm = 0;
        _cachedAccuracy = 100;
        _cachedChars = new CharacterStats(0, 0, 0, 0);
        CoreLogs.StatsReset(_logger);
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
            CoreLogs.StatsStopped(_logger, ElapsedTime.TotalMilliseconds);
        }
    }

    public GameStatisticsSnapshot CreateSnapshot()
    {
        if (_statsAreDirty)
            RecalculateAllStats();
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
        CoreLogs.RecalculatingStats(_logger);
        _cachedWpm = _keystrokeHistory.CalculateWpm(ElapsedTime);
        _cachedAccuracy = _keystrokeHistory.CalculateAccuracy();
        _cachedChars = _keystrokeHistory.GetCharacterStats();
        _statsAreDirty = false;
    }
}
