using DotNetPathUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;
using Terminal.Gui.App;
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

Log.Logger = ServiceExtensions.CreateAppLogger();
Log.Information("Application starting...");

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.AddTuiLogging();
    builder.AddTuiInfrastructure();
    builder.AddTuiScreens();

    using IHost host = builder.Build();

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
// using Terminal.Gui.App;
// using Terminal.Gui.ViewBase;
// using Terminal.Gui.Views;

// // Create the app
// using IApplication app = Application.Create();
// app.Init();

// var viewModel = new TypingViewModel();
// var win = new Window { Title = "Typing Game v2" };

// var typingView = new TypingGameView(viewModel)
// {
//     X = Pos.Center(),
//     Y = Pos.Center(),
//     Width = viewModel.TargetText.Length,
//     Height = 1,
// };

// win.Add(typingView);

// app.Run(win);
// // Shutdown is handled by Dispose in the 'using' block
