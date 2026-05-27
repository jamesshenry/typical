namespace Typical.Core;

public record TestOptions
{
    public static TestOptions Default { get; set; } = new();
    public bool ForbidIncorrectEntries { get; set; } = false;
    public bool Require100Accuracy { get; set; } = false;
    public int TargetFrameRate { get; set; } = 60;
}
