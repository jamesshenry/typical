using System.Text.Json;

using DotNetPathUtils;

using Microsoft.Extensions.Logging;

using Typical.Configuration;

using Velopack;
using Velopack.Locators;
using Velopack.Logging;

namespace Typical.Infrastructure;

public static class StartupTasks
{
    public static async Task InitializeAsync(ILogger? logger = null)
    {
        var configPath = Path.Combine(AppPaths.ConfigHome, "config.json");
        if (!File.Exists(configPath))
        {
            logger?.CreatingDefaultConfig(configPath);
            await File.WriteAllTextAsync(
                configPath,
                JsonSerializer.Serialize(new AppConfig(), AppConfigContext.Default.AppConfig)
            );
        }
    }
}
