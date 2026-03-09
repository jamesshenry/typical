namespace Build.Modules;

[DependsOn<CleanModule>]
public class NuGetUploadModule(ProjectMetadata meta, IConfiguration configuration)
    : Module<CommandResult[]>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<CommandResult[]?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var nupkgs = context.Files.Glob("dist/**.nupkg");
        if (!nupkgs.Any())
        {
            context.Logger.LogWarning("No NuGet packages found to upload.");
            return null;
        }

        var results = new List<CommandResult>();
        var apiKey = _configuration.GetValue<string>("Settings:NUGET_API_KEY");
        var count = 0;

        foreach (var package in nupkgs)
        {
            count++;

            context.Logger.LogInformation(
                "Preparing to push package: {Package} {count}/{totalCount}",
                package.Name,
                count,
                nupkgs.Count()
            );

            // SAFETY GATE: Skip the actual push if no API key or running locally without --publish
            if (string.IsNullOrEmpty(apiKey))
            {
                context.Logger.LogWarning(
                    "Skipping NuGet push for {Package} (No API Key found).",
                    package.Name
                );
                continue;
            }

            var result = await context
                .DotNet()
                .Nuget.Push(
                    new DotNetNugetPushOptions
                    {
                        Path = package,
                        Source = "https://api.nuget.org/v3/index.json",
                        ApiKey = apiKey,
                    },
                    cancellationToken: ct
                );

            results.Add(result);
        }

        return [.. results];
    }
}
