using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Typical.Core.Data;

namespace Typical.DataAccess;

[DapperAot]
internal class QuoteDto
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required string Author { get; set; }

    // SQLite returns a string, so we read a string.
    // [DbValue] maps the SQLite "Tags" column into this property automatically.
    [DbValue(Name = "Tags")]
    public string? TagsJson { get; set; }
    public int WordCount { get; set; }
    public int CharCount { get; set; }

    internal Quote ToQuote()
    {
        return new Quote
        {
            Id = Id,
            Text = Text,
            Author = Author,
            Tags =
                TagsJson?.IsWhiteSpace() == true
                    ? []
                    : JsonSerializer.Deserialize(
                        TagsJson ?? string.Empty,
                        AppJsonContext.Default.ListString
                    ) ?? [],
            WordCount = WordCount,
            CharCount = CharCount,
        };
    }
}

[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext { }
