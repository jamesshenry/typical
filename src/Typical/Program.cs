using Spectre.Console;
using Typical;
using Typical.TUI;

var game = new TypicalGame("The quick brown fox jumps over the lazy dog.");

game.Run();

// var factory = new LayoutFactory();
// var root = new Layout(LayoutName.Root.Value);
// await AnsiConsole
//     .Live(root)
//     .StartAsync(async ctx =>
//     {
//         while (true)
//         {
//             root[LayoutName.Root.Value].Update(RandomLayout());

//             ctx.Refresh();
//             await Task.Delay(3000);
//         }
//     });

// Layout RandomLayout() =>
//     Random.Shared.Next(0, 1) switch
//     {
//         1 => factory.BuildDashboard(),
//         _ => factory.BuildClassicFocus(),
//     };
