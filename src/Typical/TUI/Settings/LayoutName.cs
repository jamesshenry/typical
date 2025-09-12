using System.Diagnostics.CodeAnalysis;
using Vogen;

namespace Typical.TUI.Settings;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial record LayoutName
{
    public static readonly LayoutName Default = From(nameof(Default));
    public static readonly LayoutName Dashboard = From(nameof(Dashboard));

    public static readonly IReadOnlySet<LayoutName> All = new HashSet<LayoutName>
    {
        Default,
        Dashboard,
    };
}
