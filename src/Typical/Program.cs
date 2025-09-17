using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Typical;
using Typical.Core;
using Typical.TUI.Runtime;
using Typical.TUI.Settings;
using Velopack;

VelopackApp.Build().Run();
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
