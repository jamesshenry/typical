using Microsoft.Extensions.Time.Testing;
using Typical.Core.Statistics;

namespace Typical.Tests.Core.Statistics;

public class GameStatsTests
{
    [Test]
    public async Task CreateSnapshot_CalculatesAccurateWPM_BasedOnTime()
    {
        var fakeTime = new FakeTimeProvider();
        var stats = new GameStats(fakeTime);
        stats.Start();
        // Record 6 correct characters ("hello ")
        for (int i = 0; i < 6; i++)
            stats.RecordKey("a", KeystrokeType.Correct);
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
        var stats = new GameStats(fakeTime);
        stats.Start();
        for (int i = 0; i < 9; i++)
            stats.RecordKey("a", KeystrokeType.Correct);
        stats.RecordKey("b", KeystrokeType.Incorrect);
        fakeTime.Advance(TimeSpan.FromSeconds(10));
        stats.Stop();
        var snapshot = stats.CreateSnapshot();
        await Assert.That(snapshot.Accuracy.Value).IsEqualTo(90);
    }

    [Test]
    public async Task CreateSnapshot_HandlesZeroElapsed_PreventsDivideByZero()
    {
        var fakeTime = new FakeTimeProvider();
        var stats = new GameStats(fakeTime);
        stats.Start();
        stats.Stop();
        var snapshot = stats.CreateSnapshot();
        await Assert.That(snapshot.WPM.Value).IsEqualTo(0);
        await Assert.That(snapshot.Accuracy.Value).IsEqualTo(100);
    }
}
