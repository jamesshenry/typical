using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

[JsonConverter(typeof(JsonStringEnumConverter<HorizontalAlignment>))]
public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
}
