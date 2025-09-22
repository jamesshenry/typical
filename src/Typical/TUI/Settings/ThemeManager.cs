using Spectre.Console;
using Spectre.Console.Rendering;
using Typical.TUI.Runtime;

namespace Typical.TUI.Settings;

public class ThemeManager
{
    private RuntimeTheme _activeTheme;
    private readonly Dictionary<string, RuntimeTheme> _themes;

    public string ActiveThemeName { get; private set; }

    public ThemeManager(Dictionary<string, RuntimeTheme> themes, string? defaultTheme = null)
    {
        if (themes?.Any() != true)
            throw new ArgumentException("No themes provided.");

        _themes = themes;

        ActiveThemeName = defaultTheme ?? _themes.Keys.First();
        _activeTheme = _themes[ActiveThemeName];
    }

    public IRenderable Apply<T>(T renderable, LayoutSection layoutName)
        where T : IRenderable
    {
        if (!_activeTheme.TryGetValue(layoutName, out var style))
        {
            _activeTheme.TryGetValue(LayoutSection.Default, out style);
        }

        style ??= new ElementStyle();

        if (layoutName == LayoutSection.Header)
            style.WrapInPanel = false;

        if (!style.WrapInPanel)
            return renderable;

        Panel finalPanel;

        if (renderable is Panel existingPanel)
        {
            finalPanel = existingPanel;
        }
        else
        {
            IRenderable content = renderable;
            if (style.Alignment is not null)
            {
                var verticalAlign = Enum.Parse<Spectre.Console.VerticalAlignment>(
                    style.Alignment.Vertical.ToString(),
                    true
                );

                content = Enum.Parse<Justify>(style.Alignment.Horizontal.ToString(), true) switch
                {
                    Justify.Left => Align.Left(content, verticalAlign),
                    Justify.Center => Align.Center(content, verticalAlign),
                    Justify.Right => Align.Right(content, verticalAlign),
                    _ => renderable,
                };
            }
            finalPanel = new Panel(content);
        }

        if (style.BorderStyle is not null)
        {
            var foreground = style.BorderStyle.ForegroundColor is not null
                ? ParseColor(style.BorderStyle.ForegroundColor)
                : Color.Default;

            Enum.TryParse<Decoration>(style.BorderStyle.Decoration, true, out var decoration);

            finalPanel.BorderStyle = new Style(foreground: foreground, decoration: decoration);
        }

        if (style.PanelHeader?.Text is not null)
        {
            finalPanel.Header = new PanelHeader(style.PanelHeader.Text);
        }
        return finalPanel.Expand();
    }

    private static Color? ParseColor(string stringColor)
    {
        if (stringColor.StartsWith('#'))
            return Color.FromHex(stringColor);

        return Enum.TryParse<ConsoleColor>(stringColor, out var consoleColor)
            ? (Color?)consoleColor
            : null;
    }

    public bool TrySetTheme(string themeName)
    {
        bool exists = _themes.TryGetValue(themeName, out var theme);
        if (exists && theme is not null)
        {
            _activeTheme = theme;
        }

        return exists && theme is not null;
    }
}
