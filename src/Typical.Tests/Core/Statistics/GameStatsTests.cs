using Microsoft.Extensions.Time.Testing;
using Typical.Core.Statistics;

namespace Typical.Tests.Core.Statistics;

public class TestStatsTests
{
    [Test]
    public async Task CreateSnapshot_CalculatesAccurateWPM_BasedOnTime()
    {
        var fakeTime = new FakeTimeProvider();
        var stats = new TestSession(fakeTime);

        var builder = new TelemetryBuilder(stats, fakeTime).Type("hello ");
        fakeTime.Advance(TimeSpan.FromSeconds(12));
        stats.Stop();
        var snapshot = stats.CreateSnapshot();
        await Assert.That(snapshot.WPM.Value).IsEqualTo(6).Within(0.0001);
        await Assert.That(snapshot.Accuracy.Value).IsEqualTo(100);
    }

    [Test]
    public async Task CreateSnapshot_CalculatesAccuracy_WithErrors()
    {
        var fakeTime = new FakeTimeProvider();
        var stats = new TestSession(fakeTime);

        // Use the builder to simulate a 90% accuracy run
        new TelemetryBuilder(stats, fakeTime)
            .Type("123456789") // 9 Correct
            .Error("x"); // 1 Incorrect

        fakeTime.Advance(TimeSpan.FromSeconds(10));
        stats.Stop();

        var snapshot = stats.CreateSnapshot();

        // 9 correct / 10 physical keystrokes = 90%
        await Assert.That(snapshot.Accuracy.Value).IsEqualTo(90);
    }

    [Test]
    public async Task CreateSnapshot_HandlesZeroElapsed_PreventsDivideByZero()
    {
        var fakeTime = new FakeTimeProvider();
        var stats = new TestSession(fakeTime);
        stats.Start();
        stats.Stop();
        var snapshot = stats.CreateSnapshot();
        await Assert.That(snapshot.WPM.Value).IsEqualTo(0);
        await Assert.That(snapshot.Accuracy.Value).IsEqualTo(100);
    }
}
