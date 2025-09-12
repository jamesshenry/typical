using Typical.TUI.Runtime;

namespace Typical.TUI.Settings;

public static class AppSettingsExtensions
{
    public static RuntimeLayoutDict ToRuntimeLayouts(this LayoutPresetDict layoutDict)
    {
        return new RuntimeLayoutDict(
            layoutDict.ToDictionary(
                kvp => LayoutName.From(kvp.Key),
                kvp => kvp.Value.ToRuntimeRoot()
            )
        );
    }

    // Themes can stay string-keyed or convert similarly if needed
}
