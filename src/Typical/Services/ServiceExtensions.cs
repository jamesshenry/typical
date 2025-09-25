using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Display;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Text;
using Typical.TUI;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;
using Typical.TUI.Views;

namespace Typical.Services;

public static class ServiceExtensions
{
    public static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddJsonFile("./config.json", false).Build();

    public static void ConfigureSerilog(this ILoggingBuilder builder)
    {
        const string outputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceClass}) {Message:lj}{NewLine}{Exception}";
        builder.AddSerilog(
            new LoggerConfiguration()
                .WriteTo.File(
                    formatter: new MessageTemplateTextFormatter(outputTemplate),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log"),
                    shared: true,
                    rollingInterval: RollingInterval.Day
                )
                .Enrich.WithProperty("ApplicationName", "<APP NAME>")
                .Enrich.With<SourceClassEnricher>()
                .WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Sixteen)
                .CreateLogger()
        );
    }

    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        var configuration = CreateConfiguration();
        var appSettings = configuration.Get<AppSettings>()!;
        services.AddLogging(ConfigureSerilog);

        services.AddSingleton(configuration);

        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton(AnsiConsole.Console);
        services.AddSingleton<ThemeManager>(_ => new ThemeManager(
            appSettings.Themes.ToRuntimeThemes(),
            defaultTheme: "Default"
        )); // etc.
        services.AddSingleton<LayoutFactory>(_ => new LayoutFactory(
            appSettings.Layouts.ToRuntimeLayouts()
        ));

        // SCOPED (useful for database contexts)
        // services.AddDbContext<TypicalContext>();
        // services.AddScoped<IQuoteRepository, SqliteQuoteRepository>();
        // ... other repositories
        services.AddScoped<ITextProvider, StaticTextProvider>(_ =>
        {
            string quotePath = Path.Combine(AppContext.BaseDirectory, "quote.txt");
            string text = File.Exists(quotePath)
                ? File.ReadAllTextAsync(quotePath).Result
                : "The quick brown fox jumps over the lazy dog.";

            return new StaticTextProvider(text);
        });
        // TRANSIENT (a new instance every time)
        services.AddTransient<MarkupGenerator>();
        services.AddTransient<GameEngine>();
        services.AddTransient<AppShell>();
        services.AddTransient<MainMenuView>();
        services.AddTransient<GameView>();
        services.AddTransient<StatsView>();

        return services;
    }
}
