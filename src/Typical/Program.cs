using Typical;
using Typical.Core;
using Typical.TUI;
using Typical.TUI.Settings;

ITextProvider textProvider = new StaticTextProvider("The quick brown fox jumps over the lazy dog.");
var game = new TypicalGame(textProvider);
await game.StartNewGame();

var themeSettings = new ThemeSettings
{
    {
        LayoutName.TypingArea,
        new ElementStyle()
        {
            BorderStyle  = new BorderStyleSettings() { Decoration = "RapidBlink", ForegroundColor = "Blue"},
            PanelHeader = new PanelHeaderSettings() { Text = "Typing doodoo"},
            Alignment = "Left",
            VerticalAlignment = "Top",
        }
    },
};
var theme = new Theme(themeSettings);
var runner = new GameRunner(game, theme, LayoutConfiguration.Default);
runner.Run();
