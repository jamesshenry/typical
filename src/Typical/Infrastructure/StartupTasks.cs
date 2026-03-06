using System.Text.Json;
using DotNetPathUtils;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Typical.Configuration;
using Velopack.Locators;
using Velopack.Logging;

namespace MinCh.Infrastructure;

public static class StartupTasks
{
    private static JsonSerializerOptions JsonOptions { get; } =
        new JsonSerializerOptions { WriteIndented = true };

    internal static void Install(SemanticVersion? v = null)
    {
        var locator = VelopackLocator.CreateDefaultForPlatform();

        var logger = locator.Log;

        var installDir = locator.RootAppDir is not null
            ? Path.Combine(locator.RootAppDir, "current")
            : AppDomain.CurrentDomain.BaseDirectory;

        logger.Debug($"Adding path to $env.PATH: {installDir} ");

        var result = new PathEnvironmentHelper().EnsureDirectoryIsInPath(installDir!);

        logger.Info($"Add path result: {result.Status}");
    }

    internal static void Uninstall(SemanticVersion? v = null)
    {
        var logger = VelopackLocator.CreateDefaultForPlatform().Log;
        logger.Info("Performing installation tasks...");
        logger.Info("Cleaning up path...");

        var appDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        var result = new PathEnvironmentHelper().RemoveDirectoryFromPath(appDir);

        logger.Info($"Remove from path result: {result.Status}");
    }

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
