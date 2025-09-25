using ConsoleAppFramework;
using DotNetPathUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Typical;
using Typical.DataAccess;
using Typical.Services;
using Velopack;

var pathHelper = new PathEnvironmentHelper(
    new PathUtilsOptions()
    {
        DirectoryNameCase = DirectoryNameCase.CamelCase,
        PrefixWithPeriod = false,
    }
);
if (OperatingSystem.IsWindows())
{
    var appDirectory = Path.GetDirectoryName(AppContext.BaseDirectory)!;
    VelopackApp
        .Build()
        .OnAfterInstallFastCallback(v =>
        {
            pathHelper.EnsureDirectoryIsInPath(appDirectory);
            var dbFile = Path.Combine(appDirectory, "typical.db");
            Directory.CreateDirectory(LiteDbConstants.DataDirectory);
            File.Move(dbFile, LiteDbConstants.DbFile);
        })
        .OnBeforeUninstallFastCallback(v => pathHelper.RemoveDirectoryFromPath(appDirectory!))
        .Run();
}

var services = new ServiceCollection();

services.RegisterAppServices();

ConsoleApp.ServiceProvider = services.BuildServiceProvider();

var app = ConsoleApp.Create();

app.Add<ApplicationCommands>();

await app.RunAsync(args);
