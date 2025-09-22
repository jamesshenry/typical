namespace Typical.TUI.Settings;

public class ElementStyle
{
    public BorderStyleSettings? BorderStyle { get; set; }
    public PanelHeaderSettings? PanelHeader { get; set; }
    public AlignmentSettings? Alignment { get; set; }
    public bool WrapInPanel { get; internal set; } = true;
}
