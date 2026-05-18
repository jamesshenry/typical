using System.Threading.Tasks;
using TUnit;
using Typical.Core.Text;

namespace Typical.Tests;

public class TypingBufferTests
{
    [Test]
    public async Task Push_AddsGraphemeAndUpdatesLength()
    {
        var buffer = new TypingBuffer();
        buffer.Push("a");
        await Assert.That(buffer.GraphemeCount).IsEqualTo(1);
        await Assert.That(buffer.Length).IsEqualTo(1);
        await Assert.That(buffer.ToString()).IsEqualTo("a");
    }

    [Test]
    public async Task Pop_RemovesLastGraphemeAndUpdatesLength()
    {
        var buffer = new TypingBuffer();
        buffer.Push("a");
        buffer.Push("b");
        var popped = buffer.Pop();
        await Assert.That(popped).IsEqualTo("b");
        await Assert.That(buffer.GraphemeCount).IsEqualTo(1);
        await Assert.That(buffer.Length).IsEqualTo(1);
        await Assert.That(buffer.ToString()).IsEqualTo("a");
    }

    [Test]
    public async Task Clear_EmptiesBufferAndGraphemes()
    {
        var buffer = new TypingBuffer();
        buffer.Push("a");
        buffer.Push("b");
        buffer.Clear();
        await Assert.That(buffer.GraphemeCount).IsEqualTo(0);
        await Assert.That(buffer.Length).IsEqualTo(0);
        await Assert.That(buffer.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task GetGraphemeAt_ReturnsCorrectGrapheme()
    {
        var buffer = new TypingBuffer();
        buffer.Push("a");
        buffer.Push("b");
        await Assert.That(buffer.GetGraphemeAt(0)).IsEqualTo("a");
        await Assert.That(buffer.GetGraphemeAt(1)).IsEqualTo("b");
    }

    [Test]
    public async Task Pop_ThrowsOnEmptyBuffer()
    {
        var buffer = new TypingBuffer();
        await Assert.That(() => buffer.Pop()).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task GetGraphemeAt_ThrowsOnOutOfRange()
    {
        var buffer = new TypingBuffer();
        buffer.Push("a");
        await Assert.That(() => buffer.GetGraphemeAt(1)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Push_AllowsUnicodeGraphemes()
    {
        var buffer = new TypingBuffer();
        buffer.Push("😀");
        buffer.Push("👍🏽");
        await Assert.That(buffer.GraphemeCount).IsEqualTo(2);
        await Assert.That(buffer.ToString()).IsEqualTo("😀👍🏽");
    }
}
