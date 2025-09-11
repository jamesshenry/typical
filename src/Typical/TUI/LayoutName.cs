using System.ComponentModel;
using Vogen;

namespace Typical.TUI;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
[TypeConverter(typeof(LayoutNameTypeConverter))]
public partial record LayoutName
{
    public static readonly LayoutName Default = From(nameof(Default));

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

public class LayoutNameTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        System.Globalization.CultureInfo? culture,
        object value
    ) => value is string s ? LayoutName.From(s) : base.ConvertFrom(context, culture, value);
}
