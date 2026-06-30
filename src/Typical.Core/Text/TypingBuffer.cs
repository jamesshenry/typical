using System.Text;

namespace Typical.Core.Text;

public class TypingBuffer
{
    private readonly StringBuilder _buffer = new();

    private readonly List<string> _graphemes = new();
    public int GraphemeCount => _graphemes.Count;
    public int Length => _buffer.Length;

    public void Push(string grapheme)
    {
        _buffer.Append(grapheme);
        _graphemes.Add(grapheme);
    }

    public string Pop()
    {
        var last = _graphemes[^1];
        _buffer.Remove(_buffer.Length - last.Length, last.Length);
        _graphemes.RemoveAt(_graphemes.Count - 1);
        return last;
    }

    public void Clear()
    {
        _buffer.Clear();
        _graphemes.Clear();
    }

    public override string ToString() => _buffer.ToString();

    internal string GetGraphemeAt(int index) => _graphemes[index];
}
