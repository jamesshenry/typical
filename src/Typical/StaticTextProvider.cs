using Typical.Core.Text;

namespace Typical;

internal class StaticTextProvider(string text) : ITextProvider
{
    private readonly string _text = text;

    public Task<string> GetTextAsync() => Task.FromResult(_text);
}
