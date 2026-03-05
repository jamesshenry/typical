using Microsoft.Extensions.Logging;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Git.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;

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
#if DEBUG
        Environment.CurrentDirectory = dir.StandardOutput.Trim();
#endif
        context.Logger.LogDebug("CurrentDirectory: {Directory}", Environment.CurrentDirectory);
        context.Logger.LogDebug("Restoring slnx");
        return await context
            .DotNet()
            .Restore(
                new DotNetRestoreOptions { ProjectSolution = _meta.Solution, Runtime = _meta.Rid },
                cancellationToken: ct
            );
    }
}
