using Typical.Core.Text;

namespace Typical;

public class StaticTextProvider(string text) : ITextProvider
{
    private readonly string _text = text;

    public async Task<TextSample> GetTextAsync()
    {
        var val = new TextSample() { Text = _text, Source = "Static Text Provider" };
        return await Task.FromResult(val);
    }
}
