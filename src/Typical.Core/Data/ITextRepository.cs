namespace Typical.Core.Data;

public interface ITextRepository
{
    Task<Quote> GetRandomQuoteAsync();
    Task<Quote> GetQuoteAsync(int currentId);
    Task<bool> HasAnyAsync();
}
