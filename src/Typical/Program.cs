using System.Runtime.InteropServices;
using DotNetPathUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;
using Terminal.Gui.App;
using Typical.Core.Services;
using Typical.DataAccess;
using Typical.DataAccess.Sqlite;
using Typical.Services;
using Typical.Views;
using Velopack;

if (OperatingSystem.IsWindows())
{
    var appDirectory = Path.GetDirectoryName(AppContext.BaseDirectory)!;
    var pathHelper = new PathEnvironmentHelper(new PathUtilsOptions() { PrefixWithPeriod = false });
    VelopackApp
        .Build()
        .OnAfterInstallFastCallback(v => pathHelper.EnsureDirectoryIsInPath(appDirectory))
        .OnBeforeUninstallFastCallback(v => pathHelper.RemoveDirectoryFromPath(appDirectory!))
        .Run();
}

Log.Logger = Typical.Services.ServiceExtensions.CreateAppLogger();
Log.Information("Application starting...");

try
{
    var builder = Host.CreateApplicationBuilder(args);
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(
            new Dictionary<string, string?>()
            {
                { "ConnectionStrings:Default", TypicalDbOptions.ConnectionString },
            }
        )
        .Build();
    builder.Services.AddTypicalDb(config.GetConnectionString("Default")!);
    builder.Configuration.AddConfiguration(config);
    builder.Services.AddCoreServices();
    builder.AddTuiLogging();
    builder.AddTuiInfrastructure();
    builder.AddTuiScreens();
    using IHost host = builder.Build();

    var migrator = host.Services.GetRequiredService<IDatabaseMigrator>();

    await migrator.EnsureDatabaseUpdated();
    using var app = host.Services.GetRequiredService<IApplication>().Init();
    var mainShell = host.Services.GetRequiredService<MainShell>();

    app.Run(mainShell);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    AnsiConsole.WriteException(ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}
