using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class PanelHeaderSettings
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
