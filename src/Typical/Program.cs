using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Typical;
using Typical.Core;
using Typical.TUI;
using Typical.TUI.Settings;

var configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();

var themeSettings = configuration.GetThemeSettings();
var layouts = configuration.GetLayoutPresets();

ITextProvider textProvider = new StaticTextProvider("[[Helloooo]]");
var game = new TypicalGame(textProvider);
await game.StartNewGame();

var theme = new Theme(themeSettings);
var markupGenerator = new MarkupGenerator();
var runner = new GameRunner(game, theme, markupGenerator, AnsiConsole.Console);
runner.Run();
