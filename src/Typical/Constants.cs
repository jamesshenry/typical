namespace Typical;

public static class AppConstants
{
    public static string AppName => "Typical";

    // // Centralize the logic for getting the data directory
    // public static string DataDirectory =>
    //     Path.Combine(Xdg.Directories.BaseDirectory.DataHome, AppName.ToLower());
}

public static class AppInitializer
{
    public static void Initialize()
    {
        // if (!Directory.Exists(AppConstants.DataDirectory))
        // {
        //     Directory.CreateDirectory(AppConstants.DataDirectory);
        // }
    }
}
