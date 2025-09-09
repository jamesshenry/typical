using Typical;
using Typical.Core;
using Typical.TUI;

ITextProvider textProvider = new StaticTextProvider("The quick brown fox jumps over the lazy dog.");
var game = new TypicalGame(textProvider);
await game.StartNewGame();
var runner = new GameRunner(game, LayoutConfiguration.Default);
runner.Run();
