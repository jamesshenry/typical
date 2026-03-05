using Microsoft.Extensions.Logging;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<PublishModule>]
[DependsOn<MinVerModule>]
public class VelopackReleaseModule(ProjectMetadata meta) : Module<CommandResult>
{
    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration
            .Create()
            .WithSkipWhen(() =>
                meta.SkipPackaging
                    ? SkipDecision.Skip("Packaging explicitly skipped")
                    : SkipDecision.DoNotSkip
            )
            .Build();
    }

    protected override async Task<CommandResult?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var versionModule = await context.GetModule<MinVerModule>();
        var version = versionModule.ValueOrDefault;

        ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));
        ArgumentException.ThrowIfNullOrWhiteSpace(meta.Rid, nameof(meta.Rid));

        var root = context.Environment.WorkingDirectory;
        var publishDir = Path.Combine(root, "dist", "publish", meta.Rid);
        var releaseDir = Path.Combine(root, "dist", "release", meta.Rid);

        string directive = meta.Rid.ToLower() switch
        {
            var r when r.StartsWith("win") => "[win]",
            var r when r.StartsWith("osx") => "[osx]",
            var r when r.StartsWith("linux") => "[linux]",
            _ => throw new NotSupportedException($"RID {meta.Rid} is not supported by Velopack."),
        };

        context.Logger.LogInformation(
            "Packaging {Id} v{Version} for {Rid} using directive {Directive}",
            meta.VelopackId,
            version,
            meta.Rid,
            directive
        );

        // 5. Run Velopack (vpk)
        // Note: Using 'dotnet vpk' assumes 'vpk' is installed as a dotnet tool
        return await context.Shell.Command.ExecuteCommandLineTool(
            new VelopackOptions(rid: meta.Rid, useDnx: false)
            {
                Arguments =
                [
                    "vpk",
                    directive,
                    "pack",
                    "--packId",
                    meta.VelopackId,
                    "--packVersion",
                    version,
                    "--packDir",
                    publishDir,
                    "--outputDir",
                    releaseDir,
                    "--yes",
                ],
            },
            cancellationToken: ct
        );
    }
}
