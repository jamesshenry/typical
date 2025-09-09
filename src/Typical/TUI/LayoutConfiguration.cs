using Spectre.Console;
using Spectre.Console.Rendering;

namespace Typical.TUI;

public class LayoutConfiguration
{
    public Dictionary<LayoutName, LayoutData?> Renderables { get; } = [];

    public static LayoutConfiguration Default => new();

    public LayoutConfiguration()
    {
        Renderables[LayoutName.Header] = (
            new LayoutData(
                new Panel(new Markup("[bold yellow]Typical[/]").Centered())
                    .Header("Header")
                    .Border(BoxBorder.Rounded)
                    .Expand(),
                Align.Center
            )
        );
    }
}

public record LayoutData(
    IRenderable Content,
    Func<IRenderable, VerticalAlignment?, Align>? AlignmentFunc,
    VerticalAlignment? VerticalAlignment = null
);
