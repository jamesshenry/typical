using LiteDB;
using Typical.Core.Data;

namespace Typical.DataAccess;

public class LiteDbQuoteRepository : IQuoteRepository
{
    private readonly string _connectionString;

    // The repository takes the connection string as its dependency.
    public LiteDbQuoteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Adds a collection of quotes to the database.
    /// </summary>
    public Task AddQuotesAsync(IEnumerable<Quote> quotes)
    {
        // LiteRepository manages the connection for us.
        using var db = new LiteRepository(_connectionString);
        db.Insert(quotes);

        // LiteRepository methods are synchronous, so we wrap the call in a completed task.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fetches the next quote by ID, wrapping around if at the end.
    /// </summary>
    public async Task<Quote?> GetNextQuoteAsync(int currentId)
    {
        using var db = new LiteRepository(_connectionString);

        // Find the first quote with an ID greater than the current one.
        var nextQuote = db.Query<Quote>()
            .OrderBy(q => q.Id)
            .Where(q => q.Id > currentId)
            .Limit(1)
            .FirstOrDefault();

        if (nextQuote is null)
        {
            // If we didn't find one, wrap around and get the very first quote.
            nextQuote = db.Query<Quote>().OrderBy(q => q.Id).Limit(1).FirstOrDefault();
        }

        return await Task.FromResult(nextQuote);
    }

    /// <summary>
    /// Fetches a random quote from the collection.
    /// </summary>
    public async Task<Quote?> GetRandomQuoteAsync()
    {
        using var db = new LiteRepository(_connectionString);

        var collection = db.Database.GetCollection<Quote>();
        var count = collection.Count();

        if (count == 0)
        {
            return await Task.FromResult<Quote?>(null);
        }

        var randomIndex = Random.Shared.Next(0, count);
        var randomQuote = db.Query<Quote>().Skip(randomIndex).Limit(1).FirstOrDefault();

        return await Task.FromResult(randomQuote);
    }

    /// <summary>
    /// Checks if there is any data in the quotes collection.
    /// </summary>
    public async Task<bool> HasAnyAsync()
    {
        using var db = new LiteRepository(_connectionString);
        var hasAny = db.Query<Quote>().Exists();
        return await Task.FromResult(hasAny);
    }
}
