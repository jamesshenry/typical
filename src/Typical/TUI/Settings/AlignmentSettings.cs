using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class AlignmentSettings
{
    [JsonPropertyName("v")]
    public VerticalAlignment Vertical { get; set; }

    [JsonPropertyName("h")]
    public HorizontalAlignment Horizontal { get; set; }
}
