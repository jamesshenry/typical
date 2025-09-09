using Vogen;

namespace Typical.TUI;

[ValueObject<string>]
public partial record LayoutName
{
    // The main container for everything
    public static readonly LayoutName Root = From(nameof(Root));

    // Top-level areas
    public static readonly LayoutName Header = From(nameof(Header));
    public static readonly LayoutName TypingArea = From(nameof(TypingArea));
    public static readonly LayoutName Footer = From(nameof(Footer));

    // More specific areas, often nested within the above
    public static readonly LayoutName Breadcrumb = From(nameof(Breadcrumb));
    public static readonly LayoutName GeneralInfo = From(nameof(GeneralInfo));
    public static readonly LayoutName TypingInfo = From(nameof(TypingInfo));
    public static readonly LayoutName Center = From(nameof(Center));
}
