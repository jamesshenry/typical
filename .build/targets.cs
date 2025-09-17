#!/usr/bin/dotnet run

#:package McMaster.Extensions.CommandLineUtils@4.1.1
#:package Bullseye@6.0.0
#:package SimpleExec@12.0.0

using Bullseye;
using McMaster.Extensions.CommandLineUtils;
using static Bullseye.Targets;
using static SimpleExec.Command;

using var app = new CommandLineApplication { UsePagerForHelpText = false };
app.HelpOption();

var ridOption = app.Option<string>(
    "--rid <rid>",
    "The runtime identifier (RID) to use for publishing.",
    CommandOptionType.SingleValue
);
var versionOption = app.Option<string>(
    "--version <version>",
    "The release version.",
    CommandOptionType.SingleValue
);

app.Argument(
    "targets",
    "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
    true
);
foreach (var (aliases, description) in Options.Definitions)
{
    _ = app.Option(string.Join("|", aliases), description, CommandOptionType.NoValue);
}

app.OnExecuteAsync(async _ =>
{
    const string configuration = "Release";
    const string solution = "Typical.slnx";
    const string publishProject = "src/Typical/Typical.csproj";
    // const string packProject = "src/Typical.Core/Typical.Core.csproj";

    var root = Directory.GetCurrentDirectory();

    var targets = app.Arguments[0].Values.OfType<string>();
    var options = new Options(
        Options.Definitions.Select(d =>
            (
                d.Aliases[0],
                app.Options.Single(o => d.Aliases.Contains($"--{o.LongName}")).HasValue()
            )
        )
    );

    Target("clean", () => RunAsync("dotnet", $"clean {solution} --configuration {configuration}"));

    Target(
        "restore",
        () =>
        {
            var rid = ridOption.Value();
            var runtimeArg = !string.IsNullOrEmpty(rid) ? $"--runtime {rid}" : string.Empty;
            return RunAsync("dotnet", $"restore {solution} {runtimeArg}");
        }
    );

    Target(
        "build",
        ["restore"],
        () => RunAsync("dotnet", $"build {solution} --configuration {configuration} --no-restore")
    );

    Target(
        "test",
        ["build"],
        async () =>
        {
            var testResultFolder = "TestResults";
            var coverageFileName = "coverage.xml";
            var testResultPath = Directory.CreateDirectory(Path.Combine(root, testResultFolder));
            await RunAsync(
                "dotnet",
                $"test --solution {solution} --configuration {configuration} --coverage --coverage-output {Path.Combine(testResultPath.FullName, coverageFileName)} --coverage-output-format xml --ignore-exit-code 8"
            );
        }
    );

    Target("default", ["build"], () => Console.WriteLine("Default target ran."));

    Target(
        "publish",
        () =>
        {
            var rid = ridOption.Value();
            ArgumentException.ThrowIfNullOrWhiteSpace(rid, nameof(rid));
            var runtimeArg = $"--runtime {rid}";

            var publishDir = Path.Combine(root, "dist", "publish", rid);
            return RunAsync(
                "dotnet",
                $"publish {publishProject} -c {configuration} -o {publishDir} {runtimeArg}"
            );
        }
    );

    Target(
        "release",
        ["publish"],
        () =>
        {
            const string velopackId = "typical";
            var version = versionOption.Value();
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));
            var rid = ridOption.Value();
            ArgumentException.ThrowIfNullOrWhiteSpace(rid, nameof(rid));

            var publishDir = Path.Combine(root, "dist", "publish", rid);
            var outputDir = Path.Combine(root, "dist", "release", rid);

            var files = Directory.GetFiles(publishDir);

            foreach (var file in files)
            {
                Console.WriteLine($"FILE: {file}");
            }
            return RunAsync(
                "dotnet",
                $"vpk pack --packId {velopackId} --packVersion {version} --packDir \"{publishDir}\" --outputDir \"{outputDir}\" --yes"
            );
        }
    );


    Target(
        "upload-release",
        async () =>
        {
            var version = versionOption.Value();
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));
            var githubRepo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            ArgumentException.ThrowIfNullOrWhiteSpace(githubRepo, "GITHUB_REPOSITORY");

            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            ArgumentException.ThrowIfNullOrWhiteSpace(githubToken, "GITHUB_TOKEN");

            var velopackDir = Path.Combine(root, "dist", "artifacts", "velopack-release");
            await RunAsync(
                "vpk",
                $"upload github --repoUrl https://github.com/{githubRepo} --publish --releaseName \"Release {version}\" --tag \"v{version}\" --releaseDir \"{velopackDir}\" --token \"{githubToken}\""
            );
        }
    );

    await RunTargetsAndExitAsync(targets, options);
});

return await app.ExecuteAsync(args);
