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

// var packProjectOption = app.Option<string>(
//     "--packProject <project>",
//     "The project file to pack into a NuGet package.",
//     CommandOptionType.SingleValue,
//     opts => opts.DefaultValue = "src/CAFConsole.Lib/CAFConsole.Lib.csproj"
// );
var configurationOption = app.Option<string>(
    "-c|--configuration <configuration>",
    "The build configuration.",
    CommandOptionType.SingleValue,
    opts => opts.DefaultValue = "Release"
);
var osOption = app.Option<string>(
    "--os <os>",
    "The target operating system (e.g., win, linux, osx).",
    CommandOptionType.SingleValue
);
var archOption = app.Option<string>(
    "--arch <arch>",
    "The target architecture (e.g., x64, x86, arm64).",
    CommandOptionType.SingleValue
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

var root = Directory.GetCurrentDirectory();

app.OnExecuteAsync(async _ =>
{
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
            return RunAsync(
                "dotnet",
                $"clean {solutionOption.Value()} --configuration {configurationOption.Value()}"
            );
        }
    );

    Target(
        "restore",
        () =>
        {
            return RunAsync("dotnet", $"restore {solutionOption.Value()}");
        }
    );

    Target(
        "build",
        ["restore"],
        () =>
        {
            return RunAsync(
                "dotnet",
                $"build {solutionOption.Value()} --configuration {configurationOption.Value()} --no-restore"
            );
        }
    );

    Target(
        "test",
        ["build"],
        async () =>
        {
            var coverageFileName = "coverage.xml";
            await RunAsync(
                "dotnet",
                $"test --solution {solutionOption.Value()} --configuration {configurationOption.Value()} --no-build --ignore-exit-code 8"
            );

            var testResultFolder = "TestResults";
            string coveragePath = Path.Combine(
                root,
                "src",
                "CAFConsole.Tests",
                "bin",
                configurationOption.Value(),
                "net10.0",
                testResultFolder,
                coverageFileName
            );
            File.Move(coveragePath, Path.Combine(root, testResultFolder, coverageFileName), true);

            await RunAsync(
                "dotnet",
                $"reportgenerator -reports:{testResultFolder}/{coverageFileName} -targetdir:{testResultFolder}/coveragereport"
            );
        }
    );

    Target(
        "default",
        ["build"],
        () =>
        {
            Console.WriteLine("Default target ran, which depends on 'build'.");
        }
    );

    // Target(
    //     "pack",
    //     dependsOn: ["build"],
    //     () =>
    //     {
    //         ArgumentException.ThrowIfNullOrWhiteSpace(packProjectOption.Value());

    //         var nugetOutputDir = Path.Combine(root, "dist", "nuget"); // Example output dir

    //         return RunAsync(
    //             "dotnet",
    //             $"pack {packProjectOption.Value()} -c {configurationOption.Value()} -o {nugetOutputDir} --no-build"
    //         );
    //     }
    // );

    await RunTargetsAndExitAsync(targets, options);
});

return await app.ExecuteAsync(args);
