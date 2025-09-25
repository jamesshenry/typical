using System.Reflection;
using DotNetPathUtils;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Typical;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Text;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;
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
    var appDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
    VelopackApp
        .Build()
        .OnAfterInstallFastCallback(v => pathHelper.EnsureDirectoryIsInPath(appDirectory!))
        .OnBeforeUninstallFastCallback(v => pathHelper.RemoveDirectoryFromPath(appDirectory!))
        .Run();
}
var configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();

var appSettings = configuration.Get<AppSettings>()!;

var themeManager = new ThemeManager(appSettings.Themes.ToRuntimeThemes(), defaultTheme: "Default");
var layoutFactory = new LayoutFactory(appSettings.Layouts.ToRuntimeLayouts());

string quotePath = Path.Combine(AppContext.BaseDirectory, "quote.txt");

string text = File.Exists(quotePath)
    ? await File.ReadAllTextAsync(quotePath)
    : "The quick brown fox jumps over the lazy dog.";

ITextProvider textProvider = new StaticTextProvider(text);

var eventAggregator = new EventAggregator();
var game = new GameEngine(textProvider, eventAggregator);
var markupGenerator = new MarkupGenerator();
var runner = new TypicalGame(
    game,
    themeManager,
    markupGenerator,
    layoutFactory,
    eventAggregator,
    AnsiConsole.Console
);
await runner.RunAsync();
Console.Clear();
