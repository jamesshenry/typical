namespace Typical.Core.Data;

public interface ITextRepository
{
    Task<Quote> GetRandomQuoteAsync();
    Task<Quote> GetQuoteAsync(int currentId);
    Task<Quote> GetQuoteByIdAsync(int id);
    Task AddQuotesAsync(IEnumerable<Quote> quotes);
    Task<bool> HasAnyAsync();
}
