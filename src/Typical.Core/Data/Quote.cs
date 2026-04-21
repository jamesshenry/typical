namespace Typical.Core.Data;

public class Quote
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required string Author { get; set; }
    public IEnumerable<string> Tags { get; set; } = [];
    public int WordCount { get; set; }
    public int CharCount { get; set; }
}

public interface ITextRepository
{
    Task<Quote> GetRandomQuoteAsync();
    Task<Quote> GetQuoteAsync(int currentId);
    Task AddQuotesAsync(IEnumerable<Quote> quotes);
    Task<bool> HasAnyAsync();
}
