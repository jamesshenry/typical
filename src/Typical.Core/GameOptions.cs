namespace Typical.Core;

public record GameOptions
{
    public static GameOptions Default { get; set; } = new();
    public bool ForbidIncorrectEntries { get; set; } = false;
    public bool Require100Accuracy { get; set; } = false;
    public int TargetFrameRate { get; set; } = 60;
}
