using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DbUp;
using DbUp.Engine;

namespace Typical.DataAccess.Sqlite;

[JsonSerializable(typeof(List<QuoteSeed>))]
internal partial class SeedContext : JsonSerializerContext;

internal record QuoteSeed(string Text, string Author, List<string>? Tags);

[DbUpScript(ScriptType = DbUpScriptType.RunOnce, RunGroupOrder = 0)]
public class Script_00200_SeedInitialQuotes : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        using var cmd = dbCommandFactory();

        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM Quotes LIMIT 1)";
        if ((long)(cmd.ExecuteScalar() ?? 0) == 1)
            return "";

        var assembly = typeof(Script_00200_SeedInitialQuotes).Assembly;
        var path = Path.GetDirectoryName(assembly.Location)!;
        using var stream = File.OpenRead(Path.Combine(path, "Migrations", "quotes.json"));
        if (stream is null)
            throw new FileNotFoundException("Could not find embedded quotes.json");

        var seeds = JsonSerializer.Deserialize(stream, SeedContext.Default.ListQuoteSeed);
        if (seeds is null || seeds.Count == 0)
            return "No seeds found in JSON.";

        cmd.CommandText = "BEGIN TRANSACTION";
        cmd.ExecuteNonQuery();

        try
        {
            cmd.CommandText =
                "INSERT INTO Quotes (Text, Author, Tags) VALUES (@text, @author, @tags)";

            var pText = cmd.CreateParameter();
            pText.ParameterName = "@text";
            cmd.Parameters.Add(pText);
            var pAuthor = cmd.CreateParameter();
            pAuthor.ParameterName = "@author";
            cmd.Parameters.Add(pAuthor);
            var pTags = cmd.CreateParameter();
            pTags.ParameterName = "@tags";
            cmd.Parameters.Add(pTags);

            foreach (var quote in seeds)
            {
                pText.Value = quote.Text;
                pAuthor.Value = quote.Author ?? (object)DBNull.Value;

                // Serialize tags to JSON string for the DB
                pTags.Value =
                    quote.Tags != null
                        ? JsonSerializer.Serialize(quote.Tags, SeedContext.Default.ListString)
                        : DBNull.Value;

                cmd.ExecuteNonQuery();
            }

            cmd.CommandText = "COMMIT";
            cmd.ExecuteNonQuery();
        }
        catch
        {
            cmd.CommandText = "ROLLBACK";
            cmd.ExecuteNonQuery();
        }
        return "";
    }
}
