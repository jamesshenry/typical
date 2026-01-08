using Microsoft.Extensions.Configuration;

namespace Typical.Configuration;

public class TypicalAppConfig
{
    public int Port { get; set; }
    public bool Enabled { get; set; }

    [ConfigurationKeyName("api-url")]
    public string? ApiUrl { get; set; }
}
