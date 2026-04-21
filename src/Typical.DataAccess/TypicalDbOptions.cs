using System.Runtime.InteropServices;

namespace Typical.DataAccess;

public class TypicalDbOptions
{
    public const string SectionName = "TypicalDb";

    public string DatabaseFileName { get; set; } = "typical.db";

    public string GetDatabasePath()
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

    public string GetConnectionString() => $"Data Source={GetDatabasePath()}";
}
