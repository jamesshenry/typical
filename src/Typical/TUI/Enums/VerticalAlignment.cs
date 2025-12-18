using System.Text.Json.Serialization;

namespace Typical.TUI.Settings;

[JsonConverter(typeof(JsonStringEnumConverter<VerticalAlignment>))]
public enum VerticalAlignment
{
    Top,
    Middle,
    Bottom,
}
