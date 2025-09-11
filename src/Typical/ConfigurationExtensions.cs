using Microsoft.Extensions.Configuration;
using Typical.TUI;
using Typical.TUI.Settings;

namespace Typical;

public static class ConfigurationExtensions
{
    public static ThemeSettings GetThemeSettings(this IConfiguration configuration)
    {
        var section = configuration.GetSection("Theme");
        var dict = new ThemeSettings();

        foreach (var child in section.GetChildren())
        {
            var key = LayoutName.From(child.Key);
            var value = child.Get<ElementStyle>();
            dict[key] = value;
        }

        return dict;
    }

    // TODO:Not working
    public static LayoutPresetSettings GetLayoutPresets(this IConfiguration configuration)
    {
        var section = configuration.GetSection("Layouts");
        var dict = new LayoutPresetSettings();

        foreach (var child in section.GetChildren())
        {
            var key = child.Key;
            var value = child.Get<LayoutDefinition>();
            dict[key] = value;
        }

        return dict;
    }
}
