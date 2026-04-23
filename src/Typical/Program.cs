using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinCh.Infrastructure;
using Serilog;
using Terminal.Gui.App;
using Typical.Configuration;
using Typical.Core.Services;
using Typical.DataAccess;
using Typical.DataAccess.Sqlite;
using Typical.Services;
using Typical.Views;
using Velopack;

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
#pragma warning disable IL2026, IL3050
    await Run(host);
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

[RequiresUnreferencedCode("Calls Terminal.Gui.Application.Init(IDriver, String)")]
[RequiresDynamicCode("Calls Terminal.Gui.Application.Init(IDriver, String)")]
static async Task Run(IHost host)
{
    var migrator = host.Services.GetRequiredService<IDatabaseMigrator>();
    await migrator.EnsureDatabaseUpdated();

    using var app = host.Services.GetRequiredService<IApplication>();
    app.Init();

    var mainShell = host.Services.GetRequiredService<MainShell>();
    app.Run(mainShell);

    app.Dispose();
}
