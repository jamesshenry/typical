using System.Globalization;
using System.Text;
using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using TUnit.Core.Logging;
using Typical.Core;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Tests.Core.Statistics;

public class TypingTestTests
{
    private readonly TimeProvider _timeProvider = new FakeTimeProvider(new DateTime(2025, 01, 01));
    private readonly MockTextProvider _mockTextProvider;
    private readonly GameOptions _defaultOptions;
    private readonly GameOptions _strictOptions;
    private readonly Microsoft.Extensions.Logging.ILogger<TypingTest> _logger;
    private readonly Typical.Core.Statistics.Statistics _stats;
    private const int BOGUS_SEED = 999_999_001;
    private readonly Random _seed = new Random(BOGUS_SEED);
    private readonly DefaultLogger _testLogger;

    public TypingTestTests()
    {
        _testLogger = TestContext.Current!.GetDefaultLogger();
        Bogus.Randomizer.Seed = _seed;
        // This runs before each test, ensuring a clean state.
        _mockTextProvider = new MockTextProvider();
        _defaultOptions = new GameOptions();
        _strictOptions = new GameOptions { ForbidIncorrectEntries = true };
        _logger = NullLogger<TypingTest>.Instance;
        _stats = new Typical.Core.Statistics.Statistics(_timeProvider);
    }

    // --- StartNewGame Tests ---

    [Test]
    public async Task StartNewGame_Always_LoadsTextFromProvider()
    {
        // Arrange
        var expectedText = "This is a test.";
        _mockTextProvider.SetText(expectedText);
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);

        // Act
        sut.LoadText(await _mockTextProvider.GetWordsAsync());

        // Assert
        await Assert.That(sut.TargetText).IsEqualTo(expectedText);
    }

    [Test]
    public async Task StartNewGame_WhenGameWasAlreadyInProgress_ResetsState()
    {
        // Arrange
        // 1. Initial Setup
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        string firstText = "some text";

        // 2. Load the first sut
        sut.LoadText(new TextSample() { Text = firstText, Source = "test" });

        // 3. Simulate playing the sut
        sut.ProcessKeyPress("s", false); // Correct first char
        sut.ProcessKeyPress("o", false);

        // Check that we actually have progress
        await Assert.That(sut.UserInput).IsEqualTo("so");
        await Assert.That(sut.IsRunning).IsTrue();

        string newText = "new text";

        // Act - Loading new text should completely reset the internal state
        sut.LoadText(new TextSample() { Text = newText, Source = "test" });

        // Assert
        await Assert.That(sut.IsOver).IsFalse();
        await Assert.That(sut.IsRunning).IsFalse(); // Should not be running until first key
        await Assert.That(sut.UserInput).IsEmpty();
        await Assert.That(sut.TargetText).IsEqualTo("new text");
    }

    // --- ProcessKeyPress Tests ---

    [Test]
    public async Task ProcessKeyPress_BackspaceKey_RemovesLastCharacter()
    {
        // Arrange
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);

        sut.LoadText(new TextSample() { Text = "abc", Source = "test" });

        sut.ProcessKeyPress("a", false);
        sut.ProcessKeyPress("b", false);
        await Assert.That(sut.UserInput).IsEqualTo("ab");

        // Act
        sut.ProcessKeyPress("\0", true);

        // Assert
        await Assert.That(sut.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_BackspaceOnEmptyInput_DoesNothing()
    {
        // Arrange
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        sut.LoadText(new TextSample() { Text = "abc", Source = "test" });
        await Assert.That(sut.UserInput).IsEmpty();

        // Act
        sut.ProcessKeyPress("\0", true);

        // Assert
        await Assert.That(sut.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_WhenGameIsCompleted_SetsIsOverToTrue()
    {
        // Arrange
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        sut.LoadText(new TextSample() { Text = "hi", Source = "test" });

        // Act
        sut.ProcessKeyPress("h", false);
        sut.ProcessKeyPress("i", false);

        // Assert
        await Assert.That(sut.UserInput).IsEqualTo("hi");
        await Assert.That(sut.IsOver).IsTrue();
        await Assert.That(sut.IsRunning).IsFalse();
    }

    // --- GameOptions: ForbidIncorrectEntries (Strict Mode) Tests ---

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndCorrectKey_AppendsCharacter()
    {
        // Arrange
        var sut = new TypingTest(_strictOptions, _logger, _timeProvider); // _gameOptions.ForbidIncorrectEntries = true
        sut.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = sut.ProcessKeyPress("a", false);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(sut.UserInput).IsEqualTo("a");
    }

    [Test]
    public async Task ProcessKeyPress_InStrictModeAndIncorrectKey_DoesNotAppendCharacter()
    {
        // Arrange
        var sut = new TypingTest(_strictOptions, _logger, _timeProvider);
        sut.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = sut.ProcessKeyPress("x", false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine rejected the key
        await Assert.That(sut.UserInput).IsEmpty();
    }

    [Test]
    public async Task ProcessKeyPress_InDefaultModeAndIncorrectKey_AppendsCharacter()
    {
        // Arrange
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider); // _gameOptions.ForbidIncorrectEntries = false
        sut.LoadText(new TextSample() { Text = "abc", Source = "test" });

        // Act
        bool result = sut.ProcessKeyPress("x", false);

        // Assert
        await Assert.That(result).IsTrue(); // Engine accepted the mistake
        await Assert.That(sut.UserInput).IsEqualTo("x");
    }

    [Test]
    public async Task ProcessKeyPress_WithRandomText_MatchesState()
    {
        var lorem = new Bogus.DataSets.Lorem("ru") { Random = new Bogus.Randomizer(BOGUS_SEED) };

        var text = lorem.Sentence();

        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
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
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        sut.LoadText(new TextSample { Text = emojiText, Source = "test" });

        // Act: process the emoji as a single grapheme
        sut.ProcessKeyPress("👍🏽", false);

        // Assert: should count as exactly 1 correct keystroke
        await Assert.That(sut.Stats.Keystrokes.Count).IsEqualTo(1);
        await Assert.That(sut.Stats.Keystrokes[0].Grapheme).IsEqualTo("👍🏽");
        await Assert.That(sut.Stats.Keystrokes[0].Type).IsEqualTo(KeystrokeType.Correct);
    }

    [Test]
    [MethodDataSource(typeof(TestDataSources), nameof(TestDataSources.AdditionTestData))]
    public async Task Engine_ShouldHandleWordsInInternationalLocales(string locale)
    {
        var faker = new Faker(locale);
        var internationalText = faker.Random.Words(10);
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        sut.LoadText(new TextSample() { Text = internationalText, Source = locale });

        // Act
        var enumerator = StringInfo.GetTextElementEnumerator(internationalText);
        int graphemeCount = 0;

        while (enumerator.MoveNext())
        {
            string grapheme = enumerator.GetTextElement();

            sut.ProcessKeyPress(grapheme, false);
            graphemeCount++;
        }

        await Assert.That(sut.Stats.Keystrokes.Count).IsEqualTo(graphemeCount);
    }

    [Test]
    [MethodDataSource(typeof(TestDataSources), nameof(TestDataSources.AdditionTestData))]
    public async Task Engine_ShouldHandleSentencesInInternationalLocales(string locale)
    {
        var faker = new Faker(locale);
        var sut = new TypingTest(_defaultOptions, _logger, _timeProvider);
        var textSample = new TextSample() { Text = faker.Lorem.Sentence(), Source = locale };
        sut.LoadText(textSample);

        // Act
        var enumerator = StringInfo.GetTextElementEnumerator(textSample.Text);
        int graphemeCount = 0;

        while (enumerator.MoveNext())
        {
            string grapheme = enumerator.GetTextElement();

            sut.ProcessKeyPress(grapheme, false);
            graphemeCount++;
        }

        await Assert.That(sut.Stats.Keystrokes.Count).IsEqualTo(graphemeCount);
    }
}
