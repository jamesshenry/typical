using System.Diagnostics.Tracing;
using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public class LayoutFactory
{
    private readonly RuntimeLayoutDict _presets;

    public LayoutFactory(RuntimeLayoutDict presets)
    {
        _presets = presets;
    }

    public Layout Build(LayoutName rootLayout)
    {
        if (!_presets.TryGetValue(rootLayout, out var rootDefinition))
        {
            return new Layout(rootLayout.Value);
        }

        return BuildLayoutFromDefinition(rootDefinition, rootLayout.Value);
    }

    private Layout BuildLayoutFromDefinition(LayoutNode node, string name)
    {
        // Use root or child name
        var layout = new Layout(name);

        layout.Ratio(node.Ratio);

        if (node.Children.Count == 0)
            return layout;

        var childLayouts = node
            .Children.Select(kvp => BuildLayoutFromDefinition(kvp.Value, kvp.Key.Value))
            .ToArray();

        if (node.Direction == LayoutDirection.Rows)
            layout.SplitRows(childLayouts);
        else
            layout.SplitColumns(childLayouts);

        return layout;
    }
}
