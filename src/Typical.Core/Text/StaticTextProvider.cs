using Bogus;
using Typical.Core.Data;
using Typical.Core.Events;

namespace Typical.Core.Text;

public class StaticTextProvider(ITextRepository textRepository) : ITextProvider
{
    private readonly Faker _faker = new Faker("en_GB");

    public async Task<TextSample> GetQuoteAsync(QuoteLength length)
    {
        var result = await textRepository.GetRandomQuoteAsync();
        return new TextSample()
        {
            Source = result.Author,
            Text = result.Text,
            CharCount = result.CharCount,
            WordCount = result.WordCount,
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
