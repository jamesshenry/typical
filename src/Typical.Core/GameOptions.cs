namespace Typical.Core;

public record GameOptions
{
    public static GameOptions Default { get; set; } = new();
    public bool ForbidIncorrectEntries { get; set; } = true;
    public int TargetFrameRate { get; set; } = 60;
}
