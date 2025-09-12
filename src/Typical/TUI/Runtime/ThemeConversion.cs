using Typical.TUI.Settings;

namespace Typical.TUI.Runtime;

public static class ThemeConversion
{
    /// <summary>
    /// Converts a string-keyed Theme dictionary into a strongly-typed ThemeSettings dictionary.
    /// Validates that all keys are defined LayoutName values.
    /// </summary>
    public static RuntimeTheme ToRuntimeTheme(this Theme theme)
    {
        var result = new RuntimeTheme();

        foreach (var kvp in theme)
        {
            var layoutName = ValidateLayoutSection(kvp.Key);
            result[layoutName] = kvp.Value;
        }

        return result;
    }

    /// <summary>
    /// Converts a ThemeDict (string-keyed themes) to a runtime dictionary keyed by theme name
    /// with ThemeSettings as values.
    /// </summary>
    public static Dictionary<string, RuntimeTheme> ToRuntimeThemes(this ThemeDict themeDict)
    {
        return themeDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToRuntimeTheme());
    }

    private static LayoutSection ValidateLayoutSection(string name)
    {
        var candidate = LayoutSection.From(name);
        if (!LayoutSection.All.Contains(candidate))
        {
            throw new InvalidOperationException(
                $"Invalid layout section '{name}' in Theme. Allowed values: {string.Join(", ", LayoutName.All)}"
            );
        }

        return candidate;
    }
}

public class RuntimeTheme : Dictionary<LayoutSection, ElementStyle> { }

public class RuntimeThemeDict : Dictionary<string, RuntimeTheme> { }

public class RuntimeLayoutDict : Dictionary<LayoutName, LayoutNode>
{
    public RuntimeLayoutDict()
        : base() { }

    public RuntimeLayoutDict(Dictionary<LayoutName, LayoutNode> dictionary)
        : base(dictionary) { }
}
