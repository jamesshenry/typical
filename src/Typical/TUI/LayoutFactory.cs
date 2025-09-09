using Spectre.Console;
using static Typical.TUI.LayoutName;

namespace Typical.TUI;

public class LayoutFactory
{
    private readonly LayoutConfiguration _configuration;

    public LayoutFactory(LayoutConfiguration configuration = null!)
    {
        _configuration = configuration ?? LayoutConfiguration.Default;
    }

    public Layout Build()
    {
        return new Layout(Root.Value);
    }

    public Layout BuildClassicFocus()
    {
        return new Layout(Root.Value).SplitRows(
            new Layout(Header.Value, GetContentFor(Header)),
            new Layout(TypingArea.Value, GetContentFor(TypingArea)).Ratio(3),
            new Layout(Footer.Value, GetContentFor(Footer))
        );
    }

    public Layout BuildDashboard()
    {
        return new Layout(Root.Value).SplitColumns(
            new Layout(GeneralInfo.Value, GetContentFor(GeneralInfo)),
            new Layout(Center.Value)
                .Ratio(3)
                .SplitRows(
                    new Layout(Header.Value, GetContentFor(Header)),
                    new Layout(TypingArea.Value, GetContentFor(TypingArea)).Ratio(3),
                    new Layout(Footer.Value, GetContentFor(Footer))
                ),
            new Layout(TypingInfo.Value, GetContentFor(TypingInfo))
        );
    }

    internal Layout GetContentFor(LayoutName name)
    {
        var content = _configuration.Renderables.GetValueOrDefault(name);
        if (content is not null)
            return new Layout(
                name.Value,
                content.AlignmentFunc.Invoke(content.Content, content.VerticalAlignment)
            );

        return new Layout(name.Value, content?.Content);
    }
}
