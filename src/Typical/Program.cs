using DotNetPathUtils;
using Kuddle.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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

    builder.Configuration.Sources.Clear();

    builder.AddTuiLogging(Log.Logger);
    builder.Services.AddCoreServices();
    builder.AddTuiInfrastructure();

    builder.Services.AddTypicalDb(builder.Configuration);

    using IHost host = builder.Build();

    var migrator = host.Services.GetRequiredService<IDatabaseMigrator>();

    await migrator.EnsureDatabaseUpdated();

    using var app = host.Services.GetRequiredService<IApplication>();
    app.Init();
    var mainShell = host.Services.GetRequiredService<MainShell>();

    app.Run(mainShell);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
