using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class AppSettings
{
    [JsonPropertyName("themes")]
    public ThemeDict Themes { get; set; } = [];

    [JsonPropertyName("layouts")]
    public LayoutPresetDict Layouts { get; set; } = [];
}
