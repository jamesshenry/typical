namespace Typical.TUI.Settings;

public class LayoutDefinition
{
    public string Name { get; set; } = default!;
    public int? Ratio { get; set; } = 1;
    public string? SplitDirection { get; set; } = "Columns";
    public List<LayoutDefinition> Children { get; set; } = [];
}
