namespace Typical.TUI.Settings;

public class Theme : Dictionary<string, ElementStyle> { }

public class LayoutPresetDict : Dictionary<string, LayoutDefinition>;

public class ThemeDict : Dictionary<string, Theme> { }
