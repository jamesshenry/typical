using System.Xml.Schema;
using Spectre.Console;
using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public class LayoutFactory
{
    private readonly LayoutPresetDict _presets;

    public LayoutFactory(LayoutPresetDict presets)
    {
        _presets = presets;
    }

    public Layout Build(LayoutName rootLayout)
    {
        if (!_presets.TryGetValue(rootLayout.Value, out var rootDefinition))
        {
            return new Layout(rootLayout.Value);
        }

        return BuildLayoutFromNode(rootDefinition);
    }

    private Layout BuildLayoutFromNode(LayoutDefinition node)
    {
        var layout = new Layout(node.Section);
        layout.Ratio(node.Ratio);

        if (node.Children.Count == 0)
            return layout;

        var childLayouts = node.Children.Select(BuildLayoutFromNode).ToArray();
        if (!Enum.TryParse<LayoutDirection>(node.SplitDirection, true, out var direction))
            return layout;

        if (direction == LayoutDirection.Rows)
            layout.SplitRows(childLayouts);
        else
            layout.SplitColumns(childLayouts);

        return layout;
    }
}
