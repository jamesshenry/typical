using Typical.Core.Text;

namespace Typical.Tests;

public class MockTextProvider : ITextProvider
{
    private string _textToReturn = string.Empty;

    public void SetText(string text)
    {
        _textToReturn = text;
    }

    public Task<TextSample> GetTextAsync()
    {
        // Task.FromResult is the perfect way to simulate an
        // async operation that completes immediately.
        return Task.FromResult(new TextSample() { Source = "Tests", Text = _textToReturn });
    }
}
