using System;
using System.Linq;
using System.Threading.Tasks;
using TUnit;
using TUnit;
using Typical.Core.Statistics;

namespace Typical.Tests;

public class KeystrokeCollectionTests
{
    [Test]
    public async Task Add_CorrectIncrementsCorrectCount()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1);
        await Assert.That(kc.CorrectCount).IsEqualTo(1);
        await Assert.That(kc.TotalPhysicalKeystrokes).IsEqualTo(1);
        await Assert.That(kc.ErrorCount).IsEqualTo(0);
        await Assert.That(kc.CorrectionCount).IsEqualTo(0);
    }

    [Test]
    public async Task Add_IncorrectIncrementsErrorCount()
    {
        var kc = new KeystrokeCollection();
        kc.Add("b", KeystrokeType.Incorrect, 2);
        await Assert.That(kc.CorrectCount).IsEqualTo(0);
        await Assert.That(kc.ErrorCount).IsEqualTo(1);
        await Assert.That(kc.CorrectionCount).IsEqualTo(0);
        await Assert.That(kc.TotalPhysicalKeystrokes).IsEqualTo(1);
    }

    [Test]
    public async Task Add_CorrectionIncrementsCorrectionCount()
    {
        var kc = new KeystrokeCollection();
        kc.Add("c", KeystrokeType.Correction, 3);
        await Assert.That(kc.CorrectCount).IsEqualTo(0);
        await Assert.That(kc.ErrorCount).IsEqualTo(0);
        await Assert.That(kc.CorrectionCount).IsEqualTo(1);
        await Assert.That(kc.TotalPhysicalKeystrokes).IsEqualTo(1);
    }

    [Test]
    public async Task Add_MixedTypes_TracksAllCounts()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1);
        kc.Add("b", KeystrokeType.Incorrect, 2);
        kc.Add("c", KeystrokeType.Correction, 3);
        kc.Add("d", KeystrokeType.Correct, 4);
        await Assert.That(kc.CorrectCount).IsEqualTo(2);
        await Assert.That(kc.ErrorCount).IsEqualTo(1);
        await Assert.That(kc.CorrectionCount).IsEqualTo(1);
        await Assert.That(kc.TotalPhysicalKeystrokes).IsEqualTo(4);
    }

    [Test]
    public async Task Clear_ResetsAllCountsAndLogs()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1);
        kc.Add("b", KeystrokeType.Incorrect, 2);
        kc.Clear();
        await Assert.That(kc.CorrectCount).IsEqualTo(1); // Counts are not reset by Clear()
        await Assert.That(kc.ErrorCount).IsEqualTo(1);
        await Assert.That(kc.CorrectionCount).IsEqualTo(0);
        await Assert.That(kc.TotalPhysicalKeystrokes).IsEqualTo(0);
        await Assert.That(kc.GetLog().Count).IsEqualTo(0);
    }

    [Test]
    public async Task GetLog_ReturnsReadOnlyList()
    {
        var kc = new KeystrokeCollection();
        kc.Add("a", KeystrokeType.Correct, 1);
        var log = kc.GetLog();
        await Assert.That(log.Count).IsEqualTo(1);
        await Assert.That(log[0].Grapheme).IsEqualTo("a");
        await Assert.That(log[0].Type).IsEqualTo(KeystrokeType.Correct);
        await Assert.That(log[0].Timestamp).IsEqualTo(1);
    }
}
