using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Typical.Core.Data;
using Typical.DataAccess.Sqlite;

namespace Typical.DataAccess;

public static class ServiceExtensions
{
    public static IServiceCollection AddTypicalDb(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection(TypicalDbOptions.SectionName);
        services.Configure<TypicalDbOptions>(section);

        var options = section.Get<TypicalDbOptions>();
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        services.AddSingleton<ITextRepository, TextRepository>();
        return services;
    }
}
