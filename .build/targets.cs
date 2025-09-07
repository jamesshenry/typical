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

var solutionOption = app.Option<string>(
    "-s|--solution <solution>",
    "The solution file to operate on.",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "Typical.slnx"
);
var publishProjectOption = app.Option<string>(
    "--publishProject <project>",
    "The project file to publish as an application.",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "src/typical/typical.csproj"
);
var packProjectOption = app.Option<string>(
    "--packProject <project>",
    "The project file to pack into a NuGet package.",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "src/Typical.Core/Typical.Core.csproj"
);
var configurationOption = app.Option<string>(
    "-c|--configuration <configuration>",
    "The build configuration.",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "Release"
);
var osOption = app.Option<string>(
    "--os <os>",
    "The target operating system (e.g., win, linux, osx).",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "win"
);
var archOption = app.Option<string>(
    "--arch <arch>",
    "The target architecture (e.g., x64, x86, arm64).",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "x64"
);
var versionOption = app.Option<string>(
    "--version <version>",
    "The version to use for packing.",
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
    var root = Directory.GetCurrentDirectory();
    var configuration = configurationOption.Value();
    var solution = solutionOption.Value();

    var targets = app.Arguments[0].Values.OfType<string>();
    var options = new Options(
        Options.Definitions.Select(d =>
            (
                d.Aliases[0],
                app.Options.Single(o => d.Aliases.Contains($"--{o.LongName}")).HasValue()
            )
        )
    );

    Target(
        "clean",
        () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solution);
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration);
            return RunAsync("dotnet", $"clean {solution} --configuration {configuration}");
        }
    );

    Target(
        "restore",
        () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solution);
            return RunAsync("dotnet", $"restore {solution}");
        }
    );

    Target(
        "build",
        ["restore"],
        () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solution);
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration);
            return RunAsync(
                "dotnet",
                $"build {solution} --configuration {configuration} --no-restore"
            );
        }
    );

    Target(
        "test",
        ["build"],
        async () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solution);
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration);

            // Note: Code coverage requires .NET 10 SDK or later, and
            // the extension package Microsoft.Testing.Extensions.CodeCoverage
            var testResultFolder = "TestResults";
            var coverageFileName = "coverage.xml";
            var testResultPath = Directory.CreateDirectory(Path.Combine(root, testResultFolder));
            await RunAsync(
                "dotnet",
                $"test --solution {solution} --configuration {configuration} --no-build --coverage --coverage-output {Path.Combine(testResultPath.FullName, coverageFileName)} --coverage-output-format xml --ignore-exit-code 8"
            );
        }
    );

    Target(
        "default",
        ["build"],
        () => Console.WriteLine("Default target ran, which depends on 'build'.")
    );

    Target(
        "publish",
        dependsOn: ["build"],
        () =>
        {
            var publishProject = publishProjectOption.Value();
            var os = osOption.Value();
            var arch = archOption.Value();
            ArgumentException.ThrowIfNullOrWhiteSpace(publishProject);

            var rid = $"{os}-{arch}";

            var publishDir = Path.Combine(root, "dist", "publish", rid);

            return RunAsync(
                "dotnet",
                $"publish {publishProject} -c {configuration} -o {publishDir} --no-build"
            );
        }
    );

    Target(
        "pack",
        dependsOn: ["build"],
        async () =>
        {
            var packProject = packProjectOption.Value();

            ArgumentException.ThrowIfNullOrWhiteSpace(packProject);

            var nugetOutputDir = Path.Combine(root, "dist", "nuget");

            await RunAsync(
                "dotnet",
                $"pack {packProject} -c {configuration} -o {nugetOutputDir} --no-build"
            );

            var files = Directory.GetFiles(nugetOutputDir, "*.nupkg");
            if (files.Length == 0)
            {
                throw new InvalidOperationException("No NuGet package was created.");
            }
            foreach (var file in files)
            {
                Console.WriteLine($"NuGet package created: {file}");
            }
        }
    );

    await RunTargetsAndExitAsync(targets, options);
});

return await app.ExecuteAsync(args);
