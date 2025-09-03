using System.Text;
using Spectre.Console;

namespace Typical;

public class MarkupGenerator
{
    public Markup BuildMarkupOptimized(string target, string typed)
    {
        if (string.IsNullOrEmpty(target))
        {
            return new Markup(string.Empty);
        }

        var builder = new StringBuilder();
        var typedLength = typed.Length;
        TypingResult currentState = TypingResult.Untyped;

        if (typedLength > 0)
        {
            currentState = target[0] == typed[0] ? TypingResult.Correct : TypingResult.Incorrect;
        }
        builder.Append(GetMarkupForState(currentState));

        for (int i = 0; i < target.Length; i++)
        {
            TypingResult charState;
            if (i >= typedLength)
            {
                charState = TypingResult.Untyped;
            }
            else
            {
                charState = target[i] == typed[i] ? TypingResult.Correct : TypingResult.Incorrect;
            }

            if (charState != currentState)
            {
                builder.Append("[/]");
                builder.Append(GetMarkupForState(charState));
                currentState = charState;
            }

            builder.Append(Markup.Escape(target[i].ToString()));
        }

        builder.Append("[/]");

        if (typedLength > target.Length)
        {
            builder.Append($"[red on grey15]{Markup.Escape(typed.Substring(target.Length))}[/]");
        }

        return new Markup(builder.ToString());
    }

    private string GetMarkupForState(TypingResult state) =>
        state switch
        {
            TypingResult.Correct => "[green]",
            TypingResult.Incorrect => "[red on grey15]",
            _ => "[grey]",
        };
}
