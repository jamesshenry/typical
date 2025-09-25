using Typical.Core.Data;
using Typical.Core.Text;

namespace Typical;

public class QuoteRepositoryTextProvider : ITextProvider
{
    private readonly IQuoteRepository _quoteRepository;
    private static readonly TextSample FallbackSample = new()
    {
        Text = "The quick brown fox jumps over the lazy dog.",
        Source = "Pangram",
        WordCount = 9,
        CharCount = 43,
    };

    // It depends on the INTERFACE, not the concrete LiteDB implementation.
    public QuoteRepositoryTextProvider(IQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public async Task<TextSample> GetNextTextSampleAsync(int? currentSampleId)
    {
        if (currentSampleId is null)
        {
            return await GetTextAsync();
        }

        var quote = await _quoteRepository.GetNextQuoteAsync(currentSampleId.Value);

        return quote is null ? FallbackSample : AdaptQuoteToTextSample(quote);
    }

    public async Task<TextSample> GetTextAsync()
    {
        var quote = await _quoteRepository.GetRandomQuoteAsync();

        return quote is null ? FallbackSample : AdaptQuoteToTextSample(quote);
    }

    /// <summary>
    /// Private helper to perform the mapping from the data model to the application DTO.
    /// This is the core responsibility of the adapter pattern.
    /// </summary>
    private TextSample AdaptQuoteToTextSample(Quote quote)
    {
        return new TextSample
        {
            SourceId = quote.Id,
            Text = quote.Text,
            Source = quote.Author,
            WordCount = quote.WordCount,
            CharCount = quote.CharCount,
        };
    }
}
