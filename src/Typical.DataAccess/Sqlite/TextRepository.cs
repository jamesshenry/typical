using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Typical.Core.Data;

[assembly: DbUpGenerateScripts]

namespace Typical.DataAccess.Sqlite;

public class TextRepository : ITextRepository
{
    public Task AddQuotesAsync(IEnumerable<Quote> quotes)
    {
        throw new NotImplementedException();
    }

    public Task<Quote?> GetQuoteAsync(int currentId)
    {
        throw new NotImplementedException();
    }

    public Task<Quote?> GetRandomQuoteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasAnyAsync()
    {
        throw new NotImplementedException();
    }
}

public interface IDatabaseMigrator
{
    Task EnsureDatabaseUpdated();
}
