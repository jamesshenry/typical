using Typical.Core;

namespace Typical.Tests;

public class TypicalGameTests
{
    private readonly MockTextProvider _mockTextProvider;
    private readonly GameOptions _defaultOptions;
    private readonly GameOptions _strictOptions;

    public TypicalGameTests()
    {
        // This runs before each test, ensuring a clean state.
        _mockTextProvider = new MockTextProvider();
        _defaultOptions = new GameOptions();
        _strictOptions = new GameOptions { ForbidIncorrectEntries = true };
    }

    // --- StartNewGame Tests ---

    [Test]
    public async Task StartNewGame_Always_LoadsTextFromProvider()
    {
        // Arrange
        var expectedText = "This is a test.";
        _mockTextProvider.SetText(expectedText);
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);

        // Act
        await game.StartNewGame();

        // Assert
        await Assert.That(game.TargetText).IsEqualTo(expectedText);
    }

    [Test]
    public async Task StartNewGame_WhenGameWasAlreadyInProgress_ResetsState()
    {
        // Arrange
        _mockTextProvider.SetText("some text");
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();

        // Simulate playing the game
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        game.ProcessKeyPress(
            new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)
        );
        await Assert.That(game.IsOver).IsTrue();
        await Assert.That(game.UserInput).IsNotEmpty();

        // Act
        _mockTextProvider.SetText("new text");
        await game.StartNewGame();

        // Assert
        await Assert.That(game.IsOver).IsFalse();
        await Assert.That(game.UserInput).IsEmpty();
        await Assert.That(game.TargetText).IsEqualTo("new text");
    }

    // --- ProcessKeyPress Tests ---

    [Test]
    public async Task ProcessKeyPress_EscapeKey_EndsGameAndReturnsFalse()
    {
        // Arrange
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);

        // Act
        var result = game.ProcessKeyPress(
            new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)
        );

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(game.IsOver).IsTrue();
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceKey_RemovesLastCharacter()
    {
        // Arrange
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        game.ProcessKeyPress(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));
        await Assert.That(game.UserInput).IsEqualTo("ab");

        // Act
        game.ProcessKeyPress(
            new ConsoleKeyInfo(
                (char)ConsoleKey.Backspace,
                ConsoleKey.Backspace,
                false,
                false,
                false
            )
        );

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceOnEmptyInput_DoesNothing()
    {
        // Arrange
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(
            new ConsoleKeyInfo(
                (char)ConsoleKey.Backspace,
                ConsoleKey.Backspace,
                false,
                false,
                false
            )
        );

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_WhenGameIsCompleted_SetsIsOverToTrue()
    {
        // Arrange
        _mockTextProvider.SetText("hi");
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        game.ProcessKeyPress(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("hi");
        await Assert.That(game.IsOver).IsTrue();
    }

    // --- GameOptions: ForbidIncorrectEntries Tests ---

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndCorrectKey_AppendsCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new TypicalGame(_mockTextProvider, _strictOptions);
        await game.StartNewGame();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndIncorrectKey_DoesNotAppendCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new TypicalGame(_mockTextProvider, _strictOptions);
        await game.StartNewGame();
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_InDefaultModeAndIncorrectKey_AppendsCharacter()
    {
        // Arrange
        _mockTextProvider.SetText("abc");
        var game = new TypicalGame(_mockTextProvider, _defaultOptions);
        await game.StartNewGame();
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("x");
    }
}
