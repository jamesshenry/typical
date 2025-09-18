using System.Diagnostics;

namespace Typical.Core;

public class GameStats(TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private long? _startTimestamp;
    private long? _endTimestamp;

    public double WordsPerMinute { get; private set; }
    public double Accuracy { get; private set; }
    public bool IsRunning => _startTimestamp.HasValue && !_endTimestamp.HasValue;

    public void Start()
    {
        if (!_startTimestamp.HasValue || _endTimestamp.HasValue)
        {
            _startTimestamp = _timeProvider.GetTimestamp();
            _endTimestamp = null;
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _endTimestamp = _timeProvider.GetTimestamp();
        }
    }

    public void Update(string targetText, string typedText)
    {
        if (!IsRunning || string.IsNullOrEmpty(typedText))
        {
            WordsPerMinute = 0;
            Accuracy = 100;
            return;
        }

        long now = _timeProvider.GetTimestamp();
        var elapsed = _timeProvider.GetElapsedTime(_startTimestamp!.Value, now);
        double elapsedSeconds = elapsed.TotalSeconds;
        if (elapsedSeconds <= 0)
            return;

        var wordCount = typedText.Length / 5.0;
        WordsPerMinute = wordCount / (elapsedSeconds / 60);

        int correctChars = 0;
        for (int i = 0; i < typedText.Length; i++)
        {
            if (i < targetText.Length && typedText[i] == targetText[i])
            {
                correctChars++;
            }
        }

        Accuracy = (double)correctChars / typedText.Length * 100.0;
    }
}
