using Typical; // Your project's namespace

public class MarkupGeneratorTests
{
    private readonly MarkupGenerator _generator;

    public MarkupGeneratorTests()
    {
        // Create a new instance for each test to ensure isolation.
        _generator = new MarkupGenerator();
    }

    // --- Core Scenarios ---

    [Test]
    public async Task BuildMarkupOptimized_AllCorrectlyTyped_ReturnsFullyCorrectMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hello world";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[default on green]Hello world[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_PartiallyTypedAndCorrect_ReturnsCorrectAndUntypedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hello";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert
            .That(result)
            .IsEqualTo("[default on green]Hello[/][grey][underline] [/]world[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_WithErrors_ReturnsMixedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "Hellx worlb"; // Two errors

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // "Hell" is correct, "o" is incorrect, " worl" is correct, "d" is incorrect.
        var expected =
            "[default on green]Hell[/][red on grey15]o[/][default on green] worl[/][red on grey15]d[/]";
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task BuildMarkupOptimized_NothingTyped_ReturnsFullyUntypedMarkup()
    {
        // Arrange
        var target = "Hello world";
        var typed = "";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[grey][underline]H[/]ello world[/]");
    }

    // --- Edge Cases ---

    [Test]
    public async Task BuildMarkupOptimized_EmptyTarget_ReturnsEmptyMarkup()
    {
        // Arrange
        var target = "";
        var typed = "some input";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task BuildMarkupOptimized_UserTypedExtraCharacters_ShowsExtraCharsAsIncorrect()
    {
        // Arrange
        var target = "Hello";
        var typed = "Hello world";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // "Hello" is correct, " world" is the extra incorrect part.
        var expected = "[default on green]Hello[/][red on grey15] world[/]";
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task BuildMarkupOptimized_AllCharactersIncorrect_ReturnsFullyIncorrectMarkup()
    {
        // Arrange
        var target = "abcde";
        var typed = "fghij";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        await Assert.That(result).IsEqualTo("[red on grey15]abcde[/]");
    }

    [Test]
    public async Task BuildMarkupOptimized_TargetContainsMarkupCharacters_EscapesThemCorrectly()
    {
        // Arrange
        var target = "[[Hello]]";
        var typed = "[[Hello]]";

        // Act
        var result = _generator.BuildMarkupString(target, typed);

        // Assert
        // The generator should escape the brackets so Spectre.Console doesn't interpret them.
        await Assert.That(result).IsEqualTo("[default on green][[[[Hello]]]][/]");
    }
}
