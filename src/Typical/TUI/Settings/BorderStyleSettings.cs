using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class BorderStyleSettings
{
    [JsonPropertyName("color")]
    public string? ForegroundColor { get; set; }

    [JsonPropertyName("style")]
    public string? Decoration { get; set; }
}
