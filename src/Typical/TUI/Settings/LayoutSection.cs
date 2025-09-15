using Vogen;

namespace Typical.TUI.Settings;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial record LayoutSection
{
    public static readonly LayoutSection Default = From(nameof(Default));
    public static readonly LayoutSection Header = From(nameof(Header));
    public static readonly LayoutSection Breadcrumb = From(nameof(Breadcrumb));
    public static readonly LayoutSection TypingArea = From(nameof(TypingArea));
    public static readonly LayoutSection Footer = From(nameof(Footer));
    public static readonly LayoutSection GeneralInfo = From(nameof(GeneralInfo));
    public static readonly LayoutSection GameInfo = From(nameof(GameInfo));
    public static readonly LayoutSection TypingInfo = From(nameof(TypingInfo));
    public static readonly LayoutSection Center = From(nameof(Center));

    public static readonly IReadOnlySet<LayoutSection> All = new HashSet<LayoutSection>
    {
        Default,
        Header,
        TypingArea,
        Footer,
        Breadcrumb,
        GameInfo,
        TypingInfo,
        Center,
    };
}
