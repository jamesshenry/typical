using Build;
using Build.Modules;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines.Extensions;
using static ConsoleAppFramework.ConsoleApp;

namespace _build;

public static class Extensions
{
    extension(ProjectMetadata meta)
    {
        internal ProjectMetadata ApplyOverrides(GlobalOptions globalOptions)
        {
            // Use the provided RID or fall back to a default
            var rid = globalOptions.Rid ?? "win-x64";

            return globalOptions.Target switch
            {
                // Release & Publish: Must be Release config, packaging is active
                TargetEnum.Release or TargetEnum.Publish => meta with
                {
                    Configuration = "Release",
                    Rid = rid,
                    SkipPackaging = false,
                },

                // Test & Restore: Usually faster in Debug, packaging is skipped
                TargetEnum.Test or TargetEnum.Restore => meta with
                {
                    Configuration = "Debug",
                    Rid = rid,
                    SkipPackaging = true,
                },

                // Default Build
                TargetEnum.Build => meta with
                {
                    // If --quick is true, use Debug, otherwise Release
                    Configuration = globalOptions.Quick ? "Debug" : "Release",
                    Rid = rid,
                    SkipPackaging = globalOptions.Quick,
                },

                _ => throw new ArgumentOutOfRangeException(nameof(globalOptions.Target)),
            };
        }
    }
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServices(ProjectMetadata meta, GlobalOptions globalOptions)
        {
            services.AddSingleton(meta);

            switch (globalOptions.Target)
            {
                case TargetEnum.Release:
                    services.AddModule<VelopackReleaseModule>();
                    break;
                case TargetEnum.Publish:
                    services.AddModule<PublishModule>();
                    break;
                case TargetEnum.Test:
                    services.AddModule<TestModule>();
                    break;
                case TargetEnum.Build:
                default:
                    services.AddModule<BuildModule>();
                    break;
            }

            return services;
        }
    }
    extension(ConsoleAppBuilder appBuilder)
    {
        internal GlobalOptions BuildConsoleApp()
        {
            var globalOptions = new GlobalOptions();
            appBuilder.ConfigureGlobalOptions(
                (ref optionsBuilder) =>
                {
                    var rid = optionsBuilder.AddGlobalOption<string>("--rid");
                    var version = optionsBuilder.AddGlobalOption<string>("--version");
                    var target = optionsBuilder.AddGlobalOption<TargetEnum>("--target");
                    return new GlobalOptions(rid, version) { Target = target };
                }
            );

            appBuilder.Add(
                "",
                (ConsoleAppContext context) =>
                {
                    if (context.GlobalOptions is GlobalOptions retrieved)
                    {
                        globalOptions.UpdateFrom(retrieved);
                    }
                }
            );

            return globalOptions;
        }
    }
}

internal enum TargetEnum
{
    Build,
    Test,
    Publish,
    Restore,
    Release,
}

internal record GlobalOptions
{
    public GlobalOptions(string? rid = null, string? version = null)
    {
        Rid = rid;
        Version = version;
    }

    public string? Rid { get; set; }
    public string? Version { get; set; }
    public TargetEnum Target { get; set; } = TargetEnum.Build;
    public bool Quick { get; set; }

    public void UpdateFrom(GlobalOptions other)
    {
        Rid = other.Rid;
        Version = other.Version;
        Target = other.Target;
    }
}
