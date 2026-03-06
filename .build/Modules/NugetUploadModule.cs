namespace Build.Modules;

public class NuGetUploadModule(ProjectMetadata meta) : Module<CommandResult[]>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var nupkgs = context.Files.Glob(meta.ArtifactsDirectory, "*.nupkg");

        if (!nupkgs.Any())
        {
            context.Logger.LogWarning("No NuGet packages found to upload.");
            return null;
        }

        var results = new List<CommandResult>();
        var apiKey = context.Environment.GetEnvironmentVariable("NUGET_API_KEY");

        foreach (var package in nupkgs)
        {
            context.Logger.LogInformation("Preparing to push package: {Package}", package.Name);

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
                .NuGetPush(
                    new DotNetNuGetPushOptions
                    {
                        Path = package,
                        Source = "https://api.nuget.org/v3/index.json",
                        ApiKey = apiKey,
                    },
                    cancellationToken: ct
                );

            results.Add(result);
        }

        return results.ToArray();
    }
}
