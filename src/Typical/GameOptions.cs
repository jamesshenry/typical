namespace Typical;

public record GameOptions
{
    public static GameOptions Default { get; set; } = new();

    /// <summary>
    /// If true, the user cannot type the next character until the current one is correct.
    /// If false, the user can type ahead, and errors will be shown in red.
    /// </summary>
    public bool ForbidIncorrectEntries { get; set; } = false; // Default to original behavior
    public int TargetFrameRate { get; set; } = 60;
    // Future options could be added here:
    // public int TimeLimitSeconds { get; set; } = 0; // 0 for no limit
    // public bool ShowLiveWpm { get; set; } = false;
}
