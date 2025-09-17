using System.Reflection;
using DotNetPathUtils;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Typical;
using Typical.Core;
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
ITextProvider textProvider = new StaticTextProvider("[[Helloooo]]");

var game = new TypicalGame(textProvider);
await game.StartNewGame();
var markupGenerator = new MarkupGenerator();
var runner = new GameRunner(
    game,
    themeManager,
    markupGenerator,
    layoutFactory,
    AnsiConsole.Console
);
runner.Run();
