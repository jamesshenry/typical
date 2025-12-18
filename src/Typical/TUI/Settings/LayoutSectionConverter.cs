using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

/// <summary>
/// Custom JSON converter that handles polymorphic deserialization of layout sections.
/// Supports both string literals (e.g., "Header") and objects with nested properties.
/// </summary>
public class LayoutSectionConverter : JsonConverter<List<LayoutDefinition>>
{
    public override List<LayoutDefinition> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var result = new List<LayoutDefinition>();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected array for sections");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                // Simple string section: "Header"
                var sectionName = reader.GetString();
                result.Add(
                    new LayoutDefinition
                    {
                        Section = sectionName ?? throw new ArgumentNullException(),
                        Ratio = 1,
                        SplitDirection = "columns",
                        Children = [],
                    }
                );
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Complex object section with properties
                var definition = JsonSerializer.Deserialize<LayoutDefinition>(ref reader, options);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }
            else
            {
                throw new JsonException(
                    $"Unexpected token {reader.TokenType} in sections array. Expected string or object."
                );
            }
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<LayoutDefinition> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            // Write as simple string if it's a leaf section with default values
            if (item.Children.Count == 0 && item.Ratio == 1 && item.SplitDirection == "columns")
            {
                writer.WriteStringValue(item.Section);
            }
            else
            {
                // Write as object for complex sections
                JsonSerializer.Serialize(writer, item, options);
            }
        }

        writer.WriteEndArray();
    }
}
