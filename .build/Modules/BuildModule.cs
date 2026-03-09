namespace Build.Modules;

[DependsOn<RestoreModule>]
[DependsOn<MinVerModule>]
public class BuildModule(ProjectMetadata meta, IConfiguration configuration) : Module<CommandResult>
{
    private readonly ProjectMetadata _meta = meta;
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<CommandResult?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var version = await context.GetModule<MinVerModule>();

        return await context
            .DotNet()
            .Build(
                new DotNetBuildOptions
                {
                    ProjectSolution = _meta.MainProjectPath,
                    NoRestore = true,
                    Configuration = _meta.Configuration,
                    Properties = [new("Version", version.ValueOrDefault!)],
                    Runtime = _meta.Rid,
                },
                cancellationToken: ct
            );
    }
}
