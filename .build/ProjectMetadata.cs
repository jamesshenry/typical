namespace Build;

public record ProjectMetadata(
    string Solution,
    string MainProjectPath,
    string VelopackId,
    string? Rid = "win-x64",
    string Configuration = "Release"
)
{
    public bool SkipPackaging { get; internal set; }
    public string ArtifactsDirectory { get; internal set; } = default!;
}
