using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public static class LayoutConversion
{
    public static LayoutNode ToRuntimeRoot(this LayoutDefinition def, string? name = null)
    {
        return new LayoutNode(
            Ratio: def.Ratio ?? 1,
            Direction: ParseDirection(def.SplitDirection, name),
            Children: def.Children.ToDictionary(
                c => ValidateLayoutSectionName(c.Name),
                c => c.ToRuntimeNode()
            )
        );
    }

    private static LayoutNode ToRuntimeNode(this LayoutDefinition def)
    {
        return new LayoutNode(
            Ratio: def.Ratio ?? 1,
            Direction: ParseDirection(def.SplitDirection, def.Name),
            Children: def.Children.ToDictionary(
                c => ValidateLayoutSectionName(c.Name),
                c => c.ToRuntimeNode()
            )
        );
    }

    // Validates that a root layout name is allowed
    private static LayoutName ValidateRootLayoutName(string name)
    {
        var candidate = LayoutName.From(name);
        if (!LayoutName.All.Contains(candidate))
            throw new InvalidOperationException(
                $"Invalid root layout '{name}'. Allowed roots: {string.Join(", ", LayoutName.All)}"
            );
        return candidate;
    }

    private static LayoutSection ValidateLayoutSectionName(string name)
    {
        var candidate = LayoutSection.From(name);
        if (!LayoutSection.All.Contains(candidate))
        {
            throw new InvalidOperationException(
                $"Invalid layout name '{name}'. Allowed: {string.Join(", ", LayoutSection.All)}"
            );
        }

        return candidate;
    }

    private static LayoutDirection ParseDirection(string? raw, string? context) =>
        raw switch
        {
            "Rows" => LayoutDirection.Rows,
            "Columns" => LayoutDirection.Columns,
            _ => throw new InvalidOperationException(
                $"Invalid SplitDirection '{raw}' in layout '{context ?? "<child>"}'"
            ),
        };
}
