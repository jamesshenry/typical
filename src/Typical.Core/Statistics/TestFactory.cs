using Typical.Core.Text;

namespace Typical.Core.Statistics;

public static class TestFactory
{
    public static TestResult CreateResult(
        double wpm = 60,
        double accuracy = 100,
        TextSample? target = null,
        List<KeystrokeLog>? telemetry = null
    )
    {
        return new TestResult(
            PlayedAt: DateTime.UtcNow,
            FinalWpm: WPM.From(wpm),
            FinalAccuracy: Accuracy.From(accuracy),
            Duration: TimeSpan.FromSeconds(10),
            Target: target ?? TextSample.Empty,
            Telemetry: telemetry ?? new List<KeystrokeLog>()
        );
    }
}
