using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

public class ElementStyle
{
    [JsonPropertyName("border")]
    public BorderStyleSettings? BorderStyle { get; set; }

    [JsonPropertyName("header")]
    public PanelHeaderSettings? PanelHeader { get; set; }

    [JsonPropertyName("align")]
    public AlignmentSettings? Alignment { get; set; }

    public bool WrapInPanel { get; internal set; } = true;
}
