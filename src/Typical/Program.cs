using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinCh.Infrastructure;
using Serilog;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Typical.Configuration;
using Typical.Core.Services;
using Typical.DataAccess;
using Typical.DataAccess.Sqlite;
using Typical.Services;
using Typical.Views;
using Velopack;
using ConfigurationManager = Terminal.Gui.Configuration.ConfigurationManager;

VelopackApp.Build().Run();

Log.Logger = Typical.Services.ServiceExtensions.CreateAppLogger();
Log.Information("Application starting...");

await StartupTasks.InitializeAsync();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.Sources.Clear();

    builder.AddTuiLogging(Log.Logger);
    builder.Services.AddCoreServices();
    builder.AddTuiInfrastructure();

    builder.Services.AddTypicalDb(builder.Configuration);
    builder.Services.PostConfigure<TypicalDbOptions>(opts =>
        opts.DataDirectory = AppPaths.DataHome
    );

    using IHost host = builder.Build();

    var migrator = host.Services.GetRequiredService<IDatabaseMigrator>();

    await migrator.EnsureDatabaseUpdated();

    using var app = host.Services.GetRequiredService<IApplication>();
#pragma warning disable IL2026, IL3050
    ConfigurationManager.Enable(ConfigLocations.All);
    app.Init();
    var mainShell = host.Services.GetRequiredService<MainShell>();
    app.Run(mainShell);
#pragma warning restore IL2026, IL3050
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
