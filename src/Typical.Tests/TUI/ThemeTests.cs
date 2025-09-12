using Spectre.Console;
using Typical.TUI; // Your project's namespaces
using Typical.TUI.Settings;

public class ThemeTests
{
    // --- Basic Styling Tests ---

    [Test]
    public async Task Apply_WithSpecificStyle_SetsPanelBorderStyle()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutName.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "Blue" },
                }
            },
        };
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea");

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle).IsNotNull();
        await Assert.That(panel.BorderStyle!.Foreground).IsEqualTo(Color.Blue);
    }

    [Test]
    public async Task Apply_WithSpecificStyle_SetsPanelHeader()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutName.From("TestArea"),
                new ElementStyle { PanelHeader = new PanelHeaderSettings { Text = "Hello" } }
            },
        };
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea");

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.Header).IsNotNull();
        await Assert.That(panel.Header!.Text).IsEqualTo("Hello");
    }

    [Test]
    public async Task Apply_WithHexColor_CorrectlyParsesAndSetsColor()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutName.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "#FF00FF" },
                }
            },
        };
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea");

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Foreground).IsEqualTo(new Color(255, 0, 255));
    }

    [Test]
    public async Task Apply_WithDecoration_CorrectlyParsesAndSetsDecoration()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutName.From("TestArea"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { Decoration = "Underline" },
                }
            },
        };
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea");

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Decoration).IsEqualTo(Decoration.Underline);
    }

    // --- Fallback and Edge Case Tests ---

    [Test]
    public async Task Apply_WhenStyleIsMissing_FallsBackToDefaultStyle()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            // Note: "TestArea" is missing, but "Default" is present.
            {
                LayoutName.From("Default"),
                new ElementStyle
                {
                    BorderStyle = new BorderStyleSettings { ForegroundColor = "Red" },
                }
            },
        };
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea"); // Requesting a style that doesn't exist

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle?.Foreground).IsEqualTo(Color.Red);
    }

    [Test]
    public async Task Apply_WhenNoSpecificOrDefaultStyle_DoesNotChangePanel()
    {
        // Arrange
        var settings = new RuntimeTheme(); // Completely empty settings
        var theme = new ThemeManager(settings);
        var originalPanel = new Panel("");
        // Manually set a border to ensure it doesn't get overwritten
        originalPanel.BorderStyle = new Style(Color.Green);

        // Act
        theme.Apply(originalPanel, LayoutName.From("NonExistent"));

        // Assert
        // The panel's style should be unchanged from its original state.
        await Assert.That(originalPanel.BorderStyle?.Foreground).IsEqualTo(Color.Green);
        await Assert.That(originalPanel.Header).IsNull();
    }

    [Test]
    public async Task Apply_WithOnlyPartialStyleInfo_AppliesOnlyWhatIsProvided()
    {
        // Arrange
        var settings = new RuntimeTheme
        {
            {
                LayoutName.From("TestArea"),
                new ElementStyle { BorderStyle = new BorderStyleSettings { Decoration = "Bold" } }
            },
            // Note: ForegroundColor and PanelHeader are missing from the config.
        };
        var sttyle = new ElementStyle();
        var theme = new ThemeManager(settings);
        var panel = new Panel("");
        var layoutName = LayoutName.From("TestArea");

        // Act
        theme.Apply(panel, layoutName);

        // Assert
        await Assert.That(panel.BorderStyle).IsNotNull();
        // Foreground should be the default, not null.
        await Assert.That(panel.BorderStyle!.Foreground).IsEqualTo(Color.Default);
        await Assert.That(panel.BorderStyle.Decoration).IsEqualTo(Decoration.Bold);
        await Assert.That(panel.Header).IsNull(); // Header should not have been set.
    }

    // NOTE: The `Alignment` properties are not directly testable on the `Panel` itself,
    // because the `Apply` method returns a *new wrapper object* (`Align`) when alignment is set.
    // Testing this would require checking the type of the returned object, which is
    // more complex and often considered an implementation detail. For now, testing the
    // direct mutations of the panel provides excellent coverage of the core logic.
}
