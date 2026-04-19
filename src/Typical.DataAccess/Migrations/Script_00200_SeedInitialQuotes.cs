using System.Data;
using DbUp;
using DbUp.Engine;

namespace Typical.DataAccess.Sqlite;

[DbUpScript(ScriptType = DbUpScriptType.RunOnce, RunGroupOrder = 0)]
public class Script_00200_SeedInitialQuotes : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        using (var command = dbCommandFactory())
        {
            // We check if it's empty first to ensure idempotency
            command.CommandText = "SELECT COUNT(*) FROM Quotes";
            var count = Convert.ToInt32(command.ExecuteScalar());
            var s = new SqlScriptOptions() { RunGroupOrder = 0 };
            if (count == 0)
            {
                command.CommandText = "INSERT INTO Quotes (Text, Author) VALUES (@text, @author)";

                var pText = command.CreateParameter();
                pText.ParameterName = "@text";
                command.Parameters.Add(pText);

                var pAuthor = command.CreateParameter();
                pAuthor.ParameterName = "@author";
                command.Parameters.Add(pAuthor);

                var seeds = new[]
                {
                    ("To be, or not to be.", "Shakespeare"),
                    ("Stay hungry, stay foolish.", "Steve Jobs"),
                };

                foreach (var (text, author) in seeds)
                {
                    pText.Value = text;
                    pAuthor.Value = author;
                    command.ExecuteNonQuery();
                }
            }
        }
        return "";
    }
}
