using Spectre.Console;

namespace Typical.TUI.Settings;

public class ElementStyle
{
    public BorderStyleSettings? BorderStyle { get; set; }
    public PanelHeaderSettings? PanelHeader { get; set; }
    public string VerticalAlignment { get; set; } = nameof(Spectre.Console.VerticalAlignment.Top);
    public string Alignment { get; set; } = nameof(Spectre.Console.Align.Center);
}
