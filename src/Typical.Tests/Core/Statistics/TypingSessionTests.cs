using System.Globalization;
using System.Text;
using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TUnit.Core.Logging;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Tests.Core.Statistics;

public class TypingSessionTests
{
    private readonly MockTextProvider _mockTextProvider;
    private readonly GameOptions _defaultOptions;
    private readonly GameOptions _strictOptions;
    private readonly Microsoft.Extensions.Logging.ILogger<TypingSession> _logger;
    private readonly GameStats _stats;
    private const int BOGUS_SEED = 999_999_001;
    private readonly Random _seed = new Random(BOGUS_SEED);
    private readonly DefaultLogger _testLogger;

    public TypingSessionTests()
    {
        _testLogger = TestContext.Current!.GetDefaultLogger();
        Bogus.Randomizer.Seed = _seed;
        // This runs before each test, ensuring a clean state.
        _mockTextProvider = new MockTextProvider();
        _defaultOptions = new GameOptions();
        _strictOptions = new GameOptions { ForbidIncorrectEntries = true };
        _logger = NullLogger<TypingSession>.Instance;
        _stats = new GameStats(TimeProvider.System);
    }

    // --- StartNewGame Tests ---

    [Test]
    public async Task StartNewGame_Always_LoadsTextFromProvider()
    {
        // Arrange
        var expectedText = "This is a test.";
        _mockTextProvider.SetText(expectedText);
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);

        // Act
        game.LoadText(await _mockTextProvider.GetWordsAsync());

        // Assert
        await Assert.That(game.TargetText).IsEqualTo(expectedText);
    }

    [Test]
    public async Task StartNewGame_WhenGameWasAlreadyInProgress_ResetsState()
    {
        // Arrange
        // 1. Initial Setup
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);
        string firstText = "some text";

        // 2. Load the first game
        game.LoadText(new TextSample() { Text = firstText, Source = "test" });

        // 3. Simulate playing the game
        game.ProcessKeyPress("s", false); // Correct first char
        game.ProcessKeyPress("o", false);

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
        // await Assert.That(game.CharacterStates.All(s => s == KeystrokeType.Untyped)).IsTrue();
    }

    // --- ProcessKeyPress Tests ---

    [Test]
    public async Task ProcessKeyPress_BackspaceKey_RemovesLastCharacter()
    {
        // Arrange
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);

        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        game.ProcessKeyPress("a", false);
        game.ProcessKeyPress("b", false);
        await Assert.That(game.UserInput).IsEqualTo("ab");

        // Act - Pass '\0' or any char with isBackspace = true
        game.ProcessKeyPress("\0", true);

        // Assert
        await Assert.That(game.UserInput).IsEqualTo("a");
        // await Assert.That(game.CharacterStates[1]).IsEqualTo(KeystrokeType.Untyped);
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceOnEmptyInput_DoesNothing()
    {
        // Arrange
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });
        await Assert.That(game.UserInput).IsEmpty();

        // Act
        game.ProcessKeyPress("\0", true);

        // Assert
        await Assert.That(game.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_WhenGameIsCompleted_SetsIsOverToTrue()
    {
        // Arrange
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);
        game.LoadText(new TextSample() { Text = "hi", Source = "test" });

        // Act
        game.ProcessKeyPress("h", false);
        game.ProcessKeyPress("i", false);

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
        var game = new TypingSession(_strictOptions, _logger, TimeProvider.System); // _gameOptions.ForbidIncorrectEntries = true
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress("a", false);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(game.UserInput).IsEqualTo("a");
        // await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Correct);
    }

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndIncorrectKey_DoesNotAppendCharacter()
    {
        // Arrange
        var game = new TypingSession(_strictOptions, _logger, TimeProvider.System);
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress("x", false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine rejected the key
        await Assert.That(game.UserInput).IsEmpty();
        // await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Untyped);
    }

    [Test]
    public async Task ProcessKeyPress_InDefaultModeAndIncorrectKey_AppendsCharacter()
    {
        // Arrange
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System); // _gameOptions.ForbidIncorrectEntries = false
        game.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = game.ProcessKeyPress("x", false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine accepted the mistake
        await Assert.That(game.UserInput).IsEqualTo("x");
        // await Assert.That(game.CharacterStates[0]).IsEqualTo(KeystrokeType.Incorrect);
    }

    [Test]
    public async Task ProcessKeyPress_WithRandomText_MatchesState()
    {
        var lorem = new Bogus.DataSets.Lorem("ru") { Random = new Bogus.Randomizer(BOGUS_SEED) };

        var text = lorem.Sentence();

        var sut = new TypingSession(_defaultOptions, _logger, TimeProvider.System);
        sut.LoadText(new TextSample() { Text = text, Source = "Bogus" });

        var enumerator = StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext())
        {
            string nextGrapheme = enumerator.GetTextElement();
            bool result = sut.ProcessKeyPress(nextGrapheme, false);
            await Assert.That(result).IsTrue();
        }

        await Assert.That(sut.IsOver).IsTrue();
    }

    [Test]
    public async Task ProcessKeyPress_WithEmoji_HandlesGraphemesCorrectly()
    {
        // Arrange: emoji with modifier (👍🏽)
        var emojiText = "👍🏽";
        var game = new TypingSession(_defaultOptions, _logger, TimeProvider.System);
        game.LoadText(new TextSample { Text = emojiText, Source = "test" });

        // Act: process the emoji as a single grapheme
        game.ProcessKeyPress("👍🏽", false);

        // Assert: should count as exactly 1 correct keystroke
        await Assert.That(game.Stats.Keystrokes.Count).IsEqualTo(1);
        await Assert.That(game.Stats.Keystrokes[0].Grapheme).IsEqualTo("👍🏽");
        await Assert.That(game.Stats.Keystrokes[0].Type).IsEqualTo(KeystrokeType.Correct);
    }

    // [Test]
    // [MethodDataSource(typeof(TestDataSources), nameof(TestDataSources.AdditionTestData))]
    // public async Task Engine_ShouldHandleWordsInInternationalLocales(string locale)
    // {
    //     var faker = new Faker(locale);
    //     var internationalText = faker.Random.Words(10);
    //     var _engine = new GameEngine(_defaultOptions, _logger);
    //     _engine.LoadText(new TextSample() { Text = internationalText, Source = locale });

    //     var visualCount = new StringInfo(
    //         internationalText.Normalize(NormalizationForm.FormC)
    //     ).LengthInTextElements;
    //     await Assert.That(visualCount).IsEqualTo(_engine.CharacterStates.Count);
    // }

    // [Test]
    // [MethodDataSource(typeof(TestDataSources), nameof(TestDataSources.AdditionTestData))]
    // public async Task Engine_ShouldHandleSentencesInInternationalLocales(string locale)
    // {
    //     var faker = new Faker(locale);
    //     var _engine = new GameEngine(_defaultOptions, _logger);
    //     var textSample = new TextSample() { Text = faker.Lorem.Sentence(), Source = locale };
    //     _engine.LoadText(textSample);
    //     await _testLogger.LogDebugAsync(textSample.ToString());
    //     var visualCount = new StringInfo(
    //         textSample.Text.Normalize(NormalizationForm.FormC)
    //     ).LengthInTextElements;
    //     await Assert.That(visualCount).IsEqualTo(_engine.CharacterStates.Count);
    // }
}
