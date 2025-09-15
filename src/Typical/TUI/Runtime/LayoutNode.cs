namespace Typical.TUI.Settings;

public record LayoutNode(
    int Ratio,
    LayoutDirection Direction,
    Dictionary<LayoutSection, LayoutNode> Children
);
