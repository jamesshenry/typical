using ModularPipelines.Git.Extensions;
using ModularPipelines.Git.Options;

namespace Build.Modules;

public class RestoreModule(ProjectMetadata meta) : Module<CommandResult>
{
    private readonly ProjectMetadata _meta = meta;

    protected override async Task<CommandResult?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var dir = await context
            .Git()
            .Commands.RevParse(new GitRevParseOptions() { ShowToplevel = true }, token: ct);

        context.Logger.LogDebug("CurrentDirectory: {Directory}", Environment.CurrentDirectory);
        context.Logger.LogDebug("Restoring slnx");
        var result = await context
            .DotNet()
            .Restore(
                new DotNetRestoreOptions { ProjectSolution = _meta.Solution, Runtime = _meta.Rid },
                executionOptions: new ModularPipelines.Options.CommandExecutionOptions()
                {
                    ThrowOnNonZeroExitCode = true,
                },
                cancellationToken: ct
            );
        return result;
    }
}
