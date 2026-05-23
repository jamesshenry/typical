using Typical.Core.Text;

namespace Typical.Core.Statistics;

public readonly record struct TestResult(
    DateTime PlayedAt,
    WPM FinalWpm,
    Accuracy FinalAccuracy,
    TimeSpan Duration,
    TextSample Target,
    IReadOnlyList<KeystrokeLog> Telemetry
);
