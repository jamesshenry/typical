using Kuddle.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Terminal.Gui.App;
using Typical.Configuration;
using Typical.Core.Interfaces;
using Typical.Logging;
using Typical.Views;

namespace Typical.Services;

public static class ServiceExtensions
{
    private const string OutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceClass}) {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Creates the application logger. Call this early in Program.cs to set Log.Logger.
    /// </summary>
    public static Logger CreateAppLogger() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                formatter: new MessageTemplateTextFormatter(OutputTemplate),
                Path.Combine("logs", "app-.log"),
                restrictedToMinimumLevel: LogEventLevel.Debug,
                shared: true,
                rollingInterval: RollingInterval.Day
            )
            .Enrich.FromLogContext()
            .Enrich.With<SourceClassEnricher>()
            .CreateLogger();

    public static void AddTuiLogging(this HostApplicationBuilder builder)
    {
        builder.Services.AddSerilog();
    }

    public static void AddTuiInfrastructure(this HostApplicationBuilder builder)
    {
        builder.Configuration.Sources.Clear();

        builder.Configuration.AddKdlFile("config.kdl");
        var settings = new AppConfig();
        builder.Configuration.GetSection("tui-app-settings").Bind(settings);

        builder.Services.AddSingleton(_ => Application.Create());

        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IAppLifetime, AppLifetime>();
    }

    public static void AddTuiScreens(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<MainShell>();
        builder.Services.AddTransient<HomeView>();
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddTransient<TypingGameView>();
    }
}

public interface IAppLifetime
{
    void Quit();
}

public class AppLifetime(IApplication app) : IAppLifetime
{
    private readonly IApplication _app = app;

    public void Quit() => _app.RequestStop();
}
