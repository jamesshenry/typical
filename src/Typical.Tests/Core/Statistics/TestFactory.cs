using Typical.Core.Text;

namespace Typical.Core.Statistics;

public static class TestFactory
{
    public static TestResult CreateResult(
        double wpm = 60,
        double rawWpm = 0,
        double accuracy = 100,
        TextSample? target = null,
        List<KeystrokeLog>? telemetry = null,
        List<TestSnapshot>? snapshots = null
    )
    {
        return new TestResult(
            PlayedAt: DateTime.UtcNow,
            FinalWpm: Wpm.From(wpm),
            FinalAccuracy: Accuracy.From(accuracy),
            Duration: TimeSpan.FromSeconds(10),
            Target: target ?? TextSample.Empty,
            Telemetry: telemetry ?? new List<KeystrokeLog>(),
            Snapshots: snapshots ?? new List<TestSnapshot>(),
            RawWpm: Wpm.From(rawWpm)
        );
    }
}
