using Microsoft.Extensions.Logging;

using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Styling;
using XenoAtom.Terminal.UI.Templating;

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
        _logger.LogInformation("Starting");


        State<string?> text = new("Type here");
        State<bool> exit = new(false);
        var brush = Brush.LinearGradient(
            new GradientPoint(0f, 0f),
            new GradientPoint(1f, 0f),
            [new GradientStop(0f, Colors.DeepSkyBlue), new GradientStop(1f, Colors.White)]);

        var templates = DataTemplates.Default.Derive(builder => builder
            .Register<string>(
                DataTemplateRole.Display,
                new DataTemplate<string>(
                    Display: static (DataTemplateValue<string> value, in DataTemplateContext _) => new TextBlock(() => $"> {value.GetValue()}"),
                    Editor: null)));

        var ui = new VStack(
                new ListBox<string>().Items(["One", "Two", "Three"]),
                new Select<string>().Items(["Alpha", "Beta"]))
            .Spacing(1)
            .Style(templates);
        XTerm.Run(
                   ui,
                   onUpdate: () => exit.Value
                       ? TerminalLoopResult.StopAndKeepVisual
                       : TerminalLoopResult.Continue
                   );

    }
}
