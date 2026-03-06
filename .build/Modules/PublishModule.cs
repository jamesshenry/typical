namespace Build.Modules;

[DependsOn<RestoreModule>]
public class PublishModule(ProjectMetadata meta) : Module<CommandResult>
{
    private readonly ProjectMetadata _meta = meta;

    protected override async Task<CommandResult?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_meta.Rid, nameof(_meta.Rid));

        var publishDir = Path.Combine(
            context.Environment.WorkingDirectory,
            "dist",
            "publish",
            _meta.Rid
        );

        if (Directory.Exists(publishDir))
        {
            context.Logger.LogInformation(
                "Cleaning existing publish directory: {Path}",
                publishDir
            );
            Directory.Delete(publishDir, true);
        }

        context.Logger.LogInformation(
            "Publishing {Project} for {Rid} in {Config} mode",
            _meta.MainProjectPath,
            _meta.Rid,
            _meta.Configuration
        );

        return await context
            .DotNet()
            .Publish(
                new DotNetPublishOptions
                {
                    ProjectSolution = _meta.MainProjectPath,
                    Configuration = _meta.Configuration,
                    Output = publishDir,
                    Runtime = _meta.Rid,
                    NoRestore = true,
                },
                cancellationToken: ct
            );
    }
}
