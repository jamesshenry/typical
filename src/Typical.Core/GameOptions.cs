namespace Typical.Core;

public record GameOptions
{
    public static GameOptions Default { get; set; } = new();
    public bool ForbidIncorrectEntries { get; set; } = true;
    public int TargetFrameRate { get; set; } = 60;
    // Future options could be added here:
    // public int TimeLimitSeconds { get; set; } = 0; // 0 for no limit
    // public bool ShowLiveWpm { get; set; } = false;
}
