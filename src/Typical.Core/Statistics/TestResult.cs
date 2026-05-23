using Typical.Core.Text;

namespace Typical.Core.Statistics;

public readonly record struct TestResult(
    DateTime PlayedAt,
    WPM FinalWpm,
    Accuracy FinalAccuracy,
    TimeSpan Duration,
    TextSample TargetText,
    IReadOnlyList<KeystrokeLog> Telemetry
);
