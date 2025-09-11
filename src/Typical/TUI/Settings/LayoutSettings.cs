namespace Typical.TUI.Settings;

public class LayoutPresetSettings : Dictionary<string, LayoutDefinition> { }

public class LayoutDefinition
{
    public LayoutName Name { get; set; }
    public int? Ratio { get; set; }
    public string? SplitDirection { get; set; } // "Rows" or "Columns"
    public List<LayoutDefinition> Children { get; set; } = new();
}
