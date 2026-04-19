using System.Runtime.InteropServices;

namespace Typical.DataAccess;

public static class TypicalDbOptions
{
    public const string SectionName = "TypicalDb";

    public static string DatabaseFileName { get; set; } = "typical.db";

    public static string GetDatabasePath()
    {
        string? dataDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

        if (string.IsNullOrEmpty(dataDir))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dataDir = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                dataDir = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME")!,
                    ".local",
                    "share"
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dataDir = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME")!,
                    "Library",
                    "Application Support"
                );
            }
        }

        var finalDir = Path.Combine(dataDir ?? Path.GetTempPath(), "typical");

        if (!Directory.Exists(finalDir))
        {
            Directory.CreateDirectory(finalDir);
        }

        return Path.Combine(finalDir, DatabaseFileName);
    }

    public static string ConnectionString => $"Data Source={GetDatabasePath()}";
}
