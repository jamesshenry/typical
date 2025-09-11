using Spectre.Console;
using Spectre.Console.Rendering;

namespace Typical.TUI.Settings;

public class Theme
{
    private readonly ThemeSettings _settings;

    public Theme(ThemeSettings settings)
    {
        _settings = settings;
    }

    public void Apply(Panel panel, LayoutName layoutName)
    {
        if (!_settings.TryGetValue(layoutName, out var style))
        {
            _settings.TryGetValue(LayoutName.Default, out style);
        }
        if (style is null)
            return;

        if (style.BorderStyle is not null)
        {
            var foreground = style.BorderStyle.ForegroundColor is not null
                ? ParseColor(style.BorderStyle.ForegroundColor)
                : Color.Default;

            Enum.TryParse<Decoration>(style.BorderStyle.Decoration, true, out var decoration);

            panel.BorderStyle = new Style(foreground: foreground, decoration: decoration);
        }

        if (style.PanelHeader?.Text is not null)
        {
            panel.Header = new PanelHeader(style.PanelHeader.Text);
        }

        var verticalAlign = style.VerticalAlignment is not null
            ? Enum.Parse<VerticalAlignment>(style.VerticalAlignment, true)
            : VerticalAlignment.Top;

        if (style.Alignment is not null)
        {
            IRenderable renderable = panel;
            renderable = Enum.Parse<Justify>(style.Alignment, true) switch
            {
                Justify.Left => Align.Left(panel, verticalAlign),
                Justify.Center => Align.Center(panel, verticalAlign),
                Justify.Right => Align.Right(panel, verticalAlign),
                _ => panel,
            };
        }
    }

    private static Color? ParseColor(string stringColor)
    {
        if (stringColor.StartsWith('#'))
            return Color.FromHex(stringColor);

        return Enum.TryParse<ConsoleColor>(stringColor, out var consoleColor)
            ? (Color?)consoleColor
            : null;
    }
}
