using System.Security.Cryptography.X509Certificates;
using Build;
using Build.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Git;
using ModularPipelines.Git.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using ModularPipelines.TestHelpers;
using ModularPipelines.TestHelpers.Assertions;
using Moq;

namespace build.Tests;

public class RestoreModuleTests : TestBase
{
    private readonly Mock<IDotNet> _mockDotNet = new();
    private readonly Mock<IGit> _mockGit = new();
    private readonly ProjectMetadata _projectMetadata = new("test.slnx", "test.csproj", "test")
    {
        Rid = "win-x64",
    };

    [Before(Test)]
    public void Setup()
    {
        _mockDotNet.Invocations.Clear();
        _mockGit.Invocations.Clear();
    }

    [Test]
    public async Task RestoreModule_Should_Call_DotNet_Restore_Successfully()
    {
        // Arrange
        var gitResult = CreateSuccessfulResult("git rev-parse", "C:\\mock\\repo");
        var restoreResult = CreateSuccessfulResult("dotnet restore", "C:\\mock\\repo");

        SetupMockGit(gitResult);
        SetupMockDotNet(restoreResult);

        // Act
        var module = await await RunModuleWithMocks<RestoreModule>();

        // Assert
        await ModuleResultAssertions.AssertSuccess(module);
        VerifyDotNetRestoreCalled("test.slnx", "win-x64");
    }

    [Test]
    public async Task RestoreModule_Should_Fail_When_DotNet_Restore_Fails()
    {
        // Arrange
        var gitResult = CreateSuccessfulResult("git rev-parse", "C:\\mock\\repo");
        var failedRestoreResult = CreateFailedResult(
            "dotnet restore",
            "C:\\mock\\repo",
            "MSBUILD : error MSB1009"
        );

        SetupMockGit(gitResult);
        _mockDotNet
            .Setup(d =>
                d.Restore(
                    It.IsAny<DotNetRestoreOptions>(),
                    It.IsAny<CommandExecutionOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Mocked dotnet restore failure"));

        // Act
        // Assert

        await Assert.ThrowsAsync(async () => await await RunModuleWithMocks<RestoreModule>());
    }

    private void SetupMockGit(CommandResult result)
    {
        _mockGit
            .Setup(g =>
                g.Commands.RevParse(
                    It.IsAny<GitRevParseOptions>(),
                    It.IsAny<CommandExecutionOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
    }

    private void SetupMockDotNet(CommandResult result)
    {
        _mockDotNet
            .Setup(d =>
                d.Restore(
                    It.IsAny<DotNetRestoreOptions>(),
                    It.IsAny<CommandExecutionOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
    }

    private async Task<T> RunModuleWithMocks<T>()
        where T : class, IModule
    {
        return await RunModule<T>(
            new TestHostSettings(),
            services =>
            {
                services.RemoveAll<IDotNet>();
                services.RemoveAll<IGit>();
                services.AddSingleton(_mockGit.Object);
                services.AddSingleton(_mockDotNet.Object);
                // Don't forget the constructor dependency for RestoreModule
                services.AddSingleton(_projectMetadata);
            }
        );
    }

    private void VerifyDotNetRestoreCalled(string solution, string rid)
    {
        _mockDotNet.Verify(
            d =>
                d.Restore(
                    It.Is<DotNetRestoreOptions>(o =>
                        o.ProjectSolution == solution && o.Runtime == rid
                    ),
                    It.IsAny<CommandExecutionOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static CommandResult CreateSuccessfulResult(string input, string output) =>
        new(
            input,
            "/",
            output,
            "",
            new Dictionary<string, string?>(),
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            0
        );

    private static CommandResult CreateFailedResult(string input, string output, string error) =>
        new(
            input,
            "/",
            output,
            error,
            new Dictionary<string, string?>(),
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            1
        );
}
