using System.Text;

using Microsoft.Extensions.Logging;

using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;

using XTerm = XenoAtom.Terminal.Terminal;

namespace Typical.UI.Views;

public class MainShellXenoTerminal
{
    private readonly ILogger<MainShellXenoTerminal> _logger;

    public MainShellXenoTerminal(ILogger<MainShellXenoTerminal> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        _logger.LogInformation("Starting typing test");

        const string target = "the quick brown fox jumps over the lazy dog";
        var typedText = new State<string?>("");
        var exit = new State<bool>(false);

        // 1. The visible typing test display
        var display = new Markup(() =>
        {
            var typed = typedText.Value ?? "";
            var sb = new StringBuilder();

            for (var i = 0; i < target.Length; i++)
            {
                var ch = target[i];

                if (i < typed.Length)
                {
                    // Correct (green) or typo (white text on red)
                    if (typed[i] == ch)
                        sb.Append($"[green]{ch}[/]");
                    else
                        sb.Append($"[white on red]{ch}[/]");
                }
                else if (i == typed.Length)
                {
                    // Caret highlight on active character
                    sb.Append($"[black on white]{ch}[/]");
                }
                else
                {
                    // Untyped future letters
                    sb.Append($"[grey]{ch}[/]");
                }
            }

            return sb.ToString();
        });

        // 2. Hidden TextBox acting as the invisible keyboard driver
        var hiddenInput = new TextBox(typedText)
            .KeyDown((_, e) =>
            {
                if (e.Key == TerminalKey.Escape)
                {
                    exit.Value = true;
                    e.Handled = true;
                }
            })
            .MaxHeight(0)
            .TextInput((_, e) =>
            {
                // Prevent typing past the target sentence length
                if ((typedText.Value ?? "").Length >= target.Length)
                {
                    e.Handled = true;
                }
            });

        // 3. Stack them (hiddenInput takes 0 space)
        var root = new VStack(
            display,
            hiddenInput
        );

        XTerm.Run(
            root,
            onUpdate: () => exit.Value
                ? TerminalLoopResult.StopAndKeepVisual
                : TerminalLoopResult.Continue
        );
    }
}
