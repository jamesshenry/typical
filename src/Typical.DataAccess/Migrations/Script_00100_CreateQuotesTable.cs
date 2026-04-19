using System.Data;
using DbUp;
using DbUp.Engine;

namespace Typical.DataAccess.Sqlite;

[DbUpScript(ScriptType = DbUpScriptType.RunOnce, RunGroupOrder = 0)]
public class Script_00100_CreateQuotesTable : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        using (var command = dbCommandFactory())
        {
            command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Quotes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Text TEXT NOT NULL,
                        Author TEXT NULL
                    );
                    
                    CREATE INDEX IF NOT EXISTS IX_Quotes_Id ON Quotes(Id);";

            command.ExecuteNonQuery();
        }

        // Return a name for the journal
        return "";
    }
}
