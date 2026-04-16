using Bogus;
using Typical.Core.Events;
using Typical.Core.Text;

namespace Typical;

public class StaticTextProvider(string text) : ITextProvider
{
    private readonly Faker _faker = new Faker("en_GB");
    private readonly string _text = text;

    public async Task<TextSample> GetQuoteAsync(QuoteLength length)
    {
        return await Task.FromResult(
            new TextSample()
            {
                Text = _faker.Random.Words(_faker.Random.Int(10, 30)),
                Source = nameof(Bogus),
            }
        );
    }

    public async Task<TextSample> GetTextAsync()
    {
        var val = new TextSample() { Text = _text, Source = "Static Text Provider" };
        return await Task.FromResult(val);
    }
}
