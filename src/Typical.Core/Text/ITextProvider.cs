using Typical.Core.Events;

namespace Typical.Core.Text;

public interface ITextProvider
{
    Task<TextSample> GetQuoteAsync(QuoteLength length);
    Task<TextSample> GetWordsAsync();
}
