namespace Typical.TUI.Settings;

public class ThemeSettings : Dictionary<LayoutName, ElementStyle> { }

public class ThemeSettings2 : Dictionary<string, ElementStyle> { }

public class ThemeContainer
{
    public ThemeSettings Theme { get; set; }
}
