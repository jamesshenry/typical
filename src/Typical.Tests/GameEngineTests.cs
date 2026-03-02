using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Tests;

public class TypicalGameTests
{
    private readonly MockTextProvider _mockTextProvider;
    private readonly GameOptions _defaultOptions;
    private readonly GameOptions _strictOptions;
    private readonly ILogger<GameEngine> _logger;
    private readonly GameStats _stats;

    public TypicalGameTests()
    {
        // This runs before each test, ensuring a clean state.
        _mockTextProvider = new MockTextProvider();
        _defaultOptions = new GameOptions();
        _strictOptions = new GameOptions { ForbidIncorrectEntries = true };
        _logger = NullLogger<GameEngine>.Instance;
        _stats = new GameStats();
    }

    // --- StartNewGame Tests ---

    [Test]
    public async Task StartNewGame_Always_LoadsTextFromProvider()
    {
        // Arrange
        var expectedText = "This is a test.";
        _mockTextProvider.SetText(expectedText);
        var game = new GameEngine(_defaultOptions, _logger);

        // Act
        game.LoadText(await _mockTextProvider.GetTextAsync());

        // Assert
        await Assert.That(game.TargetText).IsEqualTo(expectedText);
    }

    [Test]
    public async Task StartNewGame_WhenGameWasAlreadyInProgress_ResetsState()
    {
        // Arrange
        // 1. Initial Setup
        var game = new GameEngine(_defaultOptions, _logger);
        string firstText = "some text";

        // 2. Load the first game
        game.LoadText(new TextSample() { Text = firstText, Source = "test" });

        // 3. Simulate playing the game
        game.ProcessKeyPress('s', false); // Correct first char
        game.ProcessKeyPress('o', false);

        // Check that we actually have progress
        await Assert.That(game.UserInput).IsEqualTo("so");
        await Assert.That(game.IsRunning).IsTrue();

        // 4. Simulate an "Abort" via the Engine's Reset/Load mechanism
        // (Esc is handled by the ViewModel, which then calls the Engine to reset)
        string newText = "new text";

        // Act - Loading new text should completely reset the internal state
        game.LoadText(new TextSample() { Text = newText, Source = "test" });

        // Assert
        await Assert.That(game.IsOver).IsFalse();
        await Assert.That(game.IsRunning).IsFalse(); // Should not be running until first key
        await Assert.That(game.UserInput).IsEmpty();
        await Assert.That(game.TargetText).IsEqualTo("new text");
        await Assert.That(game.CharacterStates.All(s => s == KeystrokeType.Untyped)).IsTrue();
    }

    // --- ProcessKeyPress Tests ---

    [Test]
    public async Task ProcessKeyPress_BackspaceKey_RemovesLastCharacter()
    {
        // Arrange
        var game = new GameEngine(_defaultOptions, _logger);

        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        game.ProcessKeyPress('a', false);
        game.ProcessKeyPress('b', false);
        await Assert.That(game.UserInput).IsEqualTo("ab");

        // Act - Pass '\0' or any char with isBackspace = true
        game.ProcessKeyPress('\0', true);

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
        await Assert.That(game.CharacterStates[1]).IsEqualTo(KeystrokeType.Untyped);
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceOnEmptyInput_DoesNothing()
    {
        // Arrange
        var game = new GameEngine(_defaultOptions, _logger);
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress('\0', true);

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_WhenGameIsCompleted_SetsIsOverToTrue()
    {
        // Arrange
        var game = new GameEngine(_defaultOptions, _logger);
        game.LoadText(new TextSample() { Text = "hi", Source = "test" });

        // Act
        game.ProcessKeyPress('h', false);
        game.ProcessKeyPress('i', false);

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("hi");
        await Assert.That(game.IsOver).IsTrue();
        await Assert.That(game.IsRunning).IsFalse();
    }

    // --- GameOptions: ForbidIncorrectEntries (Strict Mode) Tests ---

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndCorrectKey_AppendsCharacter()
    {
        // Arrange
        var game = new GameEngine(_strictOptions, _logger); // _gameOptions.ForbidIncorrectEntries = true
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress('a', false);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(game.UserInput).IsEqualTo("a");
        await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Correct);
    }

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndIncorrectKey_DoesNotAppendCharacter()
    {
        // Arrange
        var game = new GameEngine(_strictOptions, _logger);
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress('x', false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine accepted the mistake
        await Assert.That(game.UserInput).IsEqualTo("x");
        await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Incorrect);
        // Assert
        await Assert.That(result).IsFalse(); // Engine rejected the key
        await Assert.That(game.UserInput).IsEmpty();
        await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Untyped);
    }

    [Test]
    public async Task ProcessKeyPress_InDefaultModeAndIncorrectKey_AppendsCharacter()
    {
        // Arrange
        var game = new GameEngine(_defaultOptions, _logger); // _gameOptions.ForbidIncorrectEntries = false
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress('x', false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine accepted the mistake
        await Assert.That(game.UserInput).IsEqualTo("x");
        await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Incorrect);
    }
}
