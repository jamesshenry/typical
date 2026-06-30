using System.Data;
using System.Data.Common;
using System.Text.Json;
using Dapper;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typical.Core.Data;
using Typical.DataAccess.Sqlite;

namespace Typical.DataAccess.Sqlite;

public interface IDatabaseMigrator
{
    Task EnsureDatabaseUpdated();
}
