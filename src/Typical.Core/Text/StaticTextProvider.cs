using Bogus;
using Typical.Core.Data;
using Typical.Core.Events;

namespace Typical.Core.Text;

public class TextProvider(ITextRepository textRepository) : ITextProvider
{
    private readonly Faker _faker = new Faker("en_GB");

    public async Task<TextSample> GetQuoteAsync(QuoteLength? length = null)
    {
        var quote = await textRepository.GetRandomQuoteAsync();
        return new TextSample()
        {
            SourceId = quote.Id,
            Source = quote.Author,
            Text = quote.Text,
            CharCount = quote.CharCount,
            WordCount = quote.WordCount,
        };
    }

    public async Task<TextSample> GetWordsAsync()
    {
        var val = new TextSample()
        {
            Text = _faker.Random.Words(_faker.Random.Int(10, 30)),
            Source = nameof(Bogus),
        };
        return await Task.FromResult(val);
    }
}
