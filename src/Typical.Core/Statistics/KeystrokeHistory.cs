using System.Collections;

namespace Typical.Core.Statistics;

public class KeystrokeHistory : IEnumerable<KeystrokeLog>
{
    private readonly List<KeystrokeLog> _logs = new();

    public int Count => _logs.Count;
    public int CorrectCount => _logs.Count(log => log.Type == KeystrokeType.Correct);
    public int IncorrectCount => _logs.Count(log => log.Type == KeystrokeType.Incorrect);
    public int ExtraCount => _logs.Count(log => log.Type == KeystrokeType.Extra);

    private (int Correct, int Incorrect, int Extra, int Corrections) GetCounts()
    {
        int correct = 0;
        int incorrect = 0;
        int extra = 0;
        int corrections = 0;

        foreach (var log in _logs)
        {
            switch (log.Type)
            {
                case KeystrokeType.Correct:
                    correct++;
                    break;
                case KeystrokeType.Incorrect:
                    incorrect++;
                    break;
                case KeystrokeType.Extra:
                    extra++;
                    break;
                case KeystrokeType.Correction:
                    corrections++;
                    break;
            }
        }
        return (correct, incorrect, extra, corrections);
    }

    public void Add(KeystrokeLog log)
    {
        _logs.Add(log);
    }

    public void Clear()
    {
        _logs.Clear();
    }

    public double CalculateWpm(TimeSpan duration) =>
        duration.TotalMinutes == 0
            ? 0
            : _logs.Count(log => log.Type == KeystrokeType.Correct) / 5.0 / duration.TotalMinutes;

    public double CalculateAccuracy()
    {
        if (Count == 0)
            return 100.0;

        var (correct, incorrect, _, _) = GetCounts();
        int totalChars = correct + incorrect;
        return totalChars == 0 ? 100.0 : (double)correct / totalChars * 100.0;
    }

    public CharacterStats GetCharacterStats()
    {
        var counts = GetCounts();
        return new CharacterStats(
            Correct: counts.Correct,
            Incorrect: counts.Incorrect,
            Extra: counts.Extra,
            Corrections: counts.Corrections
        );
    }

    public IEnumerator<KeystrokeLog> GetEnumerator() => _logs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void RemoveLastCharacterLog()
    {
        // Use FindLastIndex to search from the end of the list.
        int indexToRemove = _logs.FindLastIndex(log =>
            log.Type == KeystrokeType.Correct
            || log.Type == KeystrokeType.Incorrect
            || log.Type == KeystrokeType.Extra
        );

        // If a log was found (index is not -1), remove it.
        if (indexToRemove != -1)
        {
            _logs.RemoveAt(indexToRemove);
        }
    }
}
