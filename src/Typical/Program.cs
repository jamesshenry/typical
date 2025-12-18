using ConsoleAppFramework;
using DotNetPathUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Spectre.Console;
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
#if DEBUG
    pathHelper.EnsureDirectoryIsInPath(appDirectory);
    var dbFile = Path.Combine(appDirectory, "typical.db");
    if (!Directory.Exists(LiteDbConstants.DataDirectory))
        Directory.CreateDirectory(LiteDbConstants.DataDirectory);
    File.Move(dbFile, LiteDbConstants.DbFile, true);
#endif
    VelopackApp
        .Build()
        .OnAfterInstallFastCallback(v =>
        {
            pathHelper.EnsureDirectoryIsInPath(appDirectory);
            var dbFile = Path.Combine(appDirectory, "typical.db");
            if (!Directory.Exists(LiteDbConstants.DataDirectory))
                Directory.CreateDirectory(LiteDbConstants.DataDirectory);
            File.Move(dbFile, LiteDbConstants.DbFile, true);
        })
        .OnBeforeUninstallFastCallback(v => pathHelper.RemoveDirectoryFromPath(appDirectory!))
        .Run();
}

var services = new ServiceCollection();

services.RegisterAppServices();

ConsoleApp.ServiceProvider = services.BuildServiceProvider();

var app = ConsoleApp.Create();

app.Add<ApplicationCommands>();
app.UseFilter<ChangeExitCodeFilter>();
await app.RunAsync(args);

internal class ChangeExitCodeFilter(ConsoleAppFilter next) : ConsoleAppFilter(next)
{
    public override async Task InvokeAsync(
        ConsoleAppContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Next.InvokeAsync(context, cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                return;
            AnsiConsole.WriteException(exception, ExceptionFormats.NoStackTrace);
        }
    }
}
