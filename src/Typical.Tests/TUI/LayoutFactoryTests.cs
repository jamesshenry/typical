// using Spectre.Console;
// using Spectre.Console.Rendering;
// using Typical.TUI.Runtime;
// using Typical.TUI.Settings;

// namespace Typical.Tests.TUI;

// public class LayoutFactoryTests
// {
//     private readonly IRenderable _testRenderable = new Text("Test Content");

//     [Test]
//     public async Task Constructor_WhenGivenNullConfiguration_DoesNotThrow()
//     {
//         // Arrange & Act
//         var factoryAction = () => new LayoutFactory(null!);

//         // Assert
//         await Assert.That(factoryAction.Invoke).ThrowsNothing();
//     }

//     [Test]
//     public async Task GetContentFor_WhenContentExistsInConfiguration_ReturnsLayoutWithCorrectNameAndRenderable()
//     {
//         // Arrange
//         var config = new RuntimeLayoutDict();
//         var factory = new LayoutFactory(config);

//         // Act
//         var resultLayout = factory.GetContentFor(LayoutSection.Header);

//         // Assert
//         await Assert.That(resultLayout).IsNotNull();
//         await Assert.That(LayoutSection.Header.Value).IsEqualTo(resultLayout.Name);
//         // await Assert.That(_testRenderable, resultLayout.Renderable).AreSame();
//     }

//     [Test]
//     public async Task GetContentFor_WhenContentDoesNotExistInConfiguration_ReturnsLayoutWithCorrectNameAndNullRenderable()
//     {
//         // Arrange
//         var config = LayoutConfiguration.Default;
//         var factory = new LayoutFactory(config);

//         // Act
//         var resultLayout = factory.GetContentFor(LayoutSection.Footer);

//         // Assert
//         await Assert.That(resultLayout).IsNotNull();
//         await Assert.That(LayoutSection.Footer.Value).IsEqualTo(resultLayout.Name);
//         // await Assert.That(resultLayout.Renderable).IsNull(); // TODO: Use IAnsiConsole TestConsole
//     }

//     [Test]
//     public async Task BuildClassicFocus_WithEmptyConfiguration_BuildsSuccessfully()
//     {
//         // Arrange
//         var factory = new LayoutFactory(LayoutConfiguration.Default);

//         // Act
//         var layout = factory.Build(LayoutName.Dashboard);

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(LayoutName.Dashboard.Value).IsEqualTo(layout.Name);
//     }

//     [Test]
//     public async Task Build_WhenRootLayoutNotInPresets_ReturnsEmptyRootLayout()
//     {
//         // Arrange
//         var factory = new LayoutFactory(new RuntimeLayoutDict());

//         // Act
//         var layout = factory.Build(LayoutName.Root);

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(layout.Name).IsEqualTo(LayoutName.Root.Value);
//         await Assert.That(layout.Children).IsEmpty();
//     }

//     [Test]
//     public async Task Build_ReturnsRootLayout()
//     {
//         // Arrange
//         var factory = new LayoutFactory();

//         // Act
//         var layout = factory.Build();

//         // Assert
//         await Assert.That(layout).IsNotNull();
//         await Assert.That(LayoutSection.Root.Value).IsEqualTo(layout.Name);
//         // await Assert.That(layout.Renderable).IsNull(); // TODO: Use IAnsiConsole TestConsole
//     }
// }
