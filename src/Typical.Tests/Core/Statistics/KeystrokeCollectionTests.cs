using Typical.Core.Statistics;

namespace Typical.Tests.Core.Statistics;

public class KeystrokeCollectionTests
{
    [Test]
    public async Task Add_CorrectIncrementsCorrectCount()
    {
        var kc = new KeystrokeCollection();
        // Updated to include the new 'index' parameter
        kc.Add("a", KeystrokeType.Correct, 1000, 0);

        await Assert.That(kc.CorrectCount).IsEqualTo(1);
        await Assert.That(kc.TotalPhysical).IsEqualTo(1);
        await Assert.That(kc.ErrorCount).IsEqualTo(0);
        await Assert.That(kc.CorrectionCount).IsEqualTo(0);
    }

    [Test]
    public async Task Add_CalculatesRelativeOffsetMs()
    {
        var kc = new KeystrokeCollection();
        long startTicks = 10_000_000; // 1 second in ticks
        long nextTicks = 15_000_000; // 1.5 seconds in ticks (500ms later)

        kc.Add("a", KeystrokeType.Correct, startTicks, 0);
        kc.Add("b", KeystrokeType.Correct, nextTicks, 1);

        var logs = kc.GetLog();

        // First key should always have 0 offset
        await Assert.That(logs[0].OffsetMs).IsEqualTo(0);
        // Second key should be 500ms offset
        await Assert.That(logs[1].OffsetMs).IsEqualTo(500);
    }

    [Test]
    public async Task Add_StoresIndexCorrectly()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1000, 42); // Targeted index 42

        var log = kc.GetLog()[0];
        await Assert.That(log.Index).IsEqualTo(42);
    }

    [Test]
    public async Task Clear_ResetsAllCountsAndLogs()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1000, 0);
        kc.Add("b", KeystrokeType.Incorrect, 2000, 1);

        kc.Clear();

        // These should now all be 0 if you fixed the KeystrokeCollection.Clear() method
        await Assert.That(kc.CorrectCount).IsEqualTo(0);
        await Assert.That(kc.ErrorCount).IsEqualTo(0);
        await Assert.That(kc.CorrectionCount).IsEqualTo(0);
        await Assert.That(kc.TotalPhysical).IsEqualTo(0);
        await Assert.That(kc.GetLog().Count).IsEqualTo(0);
    }

    [Test]
    public async Task GetLog_ReturnsDeepDataCorrectly()
    {
        var kc = new KeystrokeCollection();
        kc.Add("á", KeystrokeType.Correct, 100, 0); // Testing Unicode grapheme

        var log = kc.GetLog();
        await Assert.That(log.Count).IsEqualTo(1);
        await Assert.That(log[0].Value).IsEqualTo("á");
        await Assert.That(log[0].Type).IsEqualTo(KeystrokeType.Correct);
    }
}
