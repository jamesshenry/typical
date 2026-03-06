namespace Build.Modules;

[DependsOn<BuildModule>]
public class TestModule(ProjectMetadata meta) : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(
        IModuleContext context,
        CancellationToken ct
    )
    {
        var testResultsDir = Path.Combine(context.Environment.WorkingDirectory, "TestResults");
        var coverageFilePath = Path.Combine(testResultsDir, "coverage.xml");

        Directory.CreateDirectory(testResultsDir);

        context.Logger.LogInformation("Running tests for {Solution}", meta.Solution);

        return await context
            .DotNet()
            .Test(
                new DotNetTestOptions
                {
                    Solution = meta.Solution,
                    Configuration = meta.Configuration,
                    NoBuild = true, // Since we depend on BuildModule
                    // Using Arguments to pass the specific coverage flags not covered by the default Options object
                    Arguments =
                    [
                        "--coverage",
                        "--coverage-output",
                        coverageFilePath,
                        "--coverage-output-format",
                        "xml",
                        "--ignore-exit-code",
                        "8",
                    ],
                },
                cancellationToken: ct
            );
    }
}
