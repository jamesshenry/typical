namespace Typical.DataAccess;

public static class LiteDbConstants
{
    static LiteDbConstants()
    {
        string? dataDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

        if (dataDir is null)
        {
            if (OperatingSystem.IsWindows())
            {
                dataDir = Environment.GetEnvironmentVariable("LOCALAPPDATA")!;
            }
            else if (OperatingSystem.IsLinux())
            {
                dataDir = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME")!,
                    ".local",
                    "share"
                );
            }
            else if (OperatingSystem.IsMacOS())
            {
                dataDir = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME")!,
                    "Library",
                    "Application Support"
                );
            }
        }
        DataDirectory = Path.Combine(dataDir!, "typical");
    }

    public static string DataDirectory { get; }
    public static string DbFile => Path.Combine(DataDirectory, "typical.db");
    public static string ConnectionString => $"Filename={DbFile}";
}
