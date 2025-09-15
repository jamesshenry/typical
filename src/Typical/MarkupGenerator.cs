using System.Text;
using Spectre.Console;

namespace Typical;

public class MarkupGenerator
{
    public Markup BuildMarkupOptimized(string target, string typed)
    {
        return new Markup(BuildMarkupString(target, typed));
    }

    internal string BuildMarkupString(string target, string typed)
    {
        if (string.IsNullOrEmpty(target))
        {
            return string.Empty;
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
            builder.Append(GetMarkupForState(TypingResult.Incorrect));
            builder.Append($"{Markup.Escape(typed.Substring(target.Length))}");
            builder.Append("[/]");
        }

        return builder.ToString();
    }

    private string GetMarkupForState(TypingResult state) =>
        state switch
        {
            TypingResult.Correct => "[default on green]",
            TypingResult.Incorrect => "[red on grey15]",
            _ => "[grey]",
        };
}
