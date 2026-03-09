using ModularPipelines.Git.Extensions;
using ModularPipelines.Git.Options;

namespace Build.Modules;

public class CleanModule : Module<bool>
{
    protected override async Task<bool> ExecuteAsync(IModuleContext context, CancellationToken ct)
    {
#if DEBUG
        var dir = await context
            .Git()
            .Commands.RevParse(new GitRevParseOptions() { ShowToplevel = true }, token: ct);

        context.Environment.WorkingDirectory = dir.StandardOutput.Trim();
#endif
        var artifacts = context.Files.GetFolder(".artifacts");
        context.Logger.LogInformation("Removing {artifacts} folder.", artifacts);
        var dist = context.Files.GetFolder(".dist");
        context.Logger.LogInformation("Removing {dist} folder.", dist);

        return true;
    }
}
