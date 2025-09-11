using Typical.Core;

namespace Typical.Tests;

public class MockTextProvider : ITextProvider
{
    private string _textToReturn = string.Empty;

    public void SetText(string text)
    {
        _textToReturn = text;
    }

    public Task<string> GetTextAsync()
    {
        // Task.FromResult is the perfect way to simulate an
        // async operation that completes immediately.
        return Task.FromResult(_textToReturn);
    }
}
