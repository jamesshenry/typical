namespace Typical.DataAccess;

public class TypicalDbOptions
{
    public const string SectionName = "TypicalDb";

    public string DatabaseFileName { get; set; } = "typical.db";

    public string DataDirectory { get; set; } = Path.GetTempPath();

    public string GetDatabasePath() => Path.Combine(DataDirectory, DatabaseFileName);

    public string GetConnectionString() => $"Data Source={GetDatabasePath()}";
}
