using System.Text;

namespace Typical.Core.Text;

public class TypingBuffer
{
    private readonly StringBuilder _buffer = new();
    private readonly Stack<int> _graphemeLengths = new();

    public int GraphemeCount => _graphemeLengths.Count;
    public int Length => _buffer.Length;

    public void Push(string grapheme)
    {
        _buffer.Append(grapheme);
        _graphemeLengths.Push(grapheme.Length);
    }

    public string Pop()
    {
        if (_graphemeLengths.Count == 0)
            return string.Empty;

        int len = _graphemeLengths.Pop();
        // Extract the string we are about to delete (useful for logs/logic)
        string removed = _buffer.ToString(_buffer.Length - len, len);
        _buffer.Remove(_buffer.Length - len, len);

        return removed;
    }

    public void Clear()
    {
        _buffer.Clear();
        _graphemeLengths.Clear();
    }

    public override string ToString() => _buffer.ToString();
}
