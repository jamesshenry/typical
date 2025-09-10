using Spectre.Console;
using Spectre.Console.Rendering;

namespace Typical.TUI;

public class LayoutConfiguration
{
    public Dictionary<LayoutName, IRenderable?> Renderables { get; } = [];

    public static LayoutConfiguration Default => new();

    public LayoutConfiguration()
    {
        Renderables[LayoutName.Header] = new Markup("[bold yellow]Typical[/]");
    }
}
