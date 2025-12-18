using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class LayoutDefinition
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = default!;

    [JsonPropertyName("size")]
    public int Ratio { get; set; } = 1;

    [JsonPropertyName("split")]
    public string SplitDirection { get; set; } = "columns";

    [JsonPropertyName("children")]
    public List<LayoutDefinition> Children { get; set; } = [];
}
