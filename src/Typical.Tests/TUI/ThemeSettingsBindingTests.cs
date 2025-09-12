using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TUnit;
using Typical.TUI.Settings;

namespace Typical.TUI.Tests;

public class ThemeSettingsBindingTests
{
    [Test]
    public async Task Can_Bind_ThemeSettings_With_LayoutName_Keys()
    {
        // Arrange: fake config JSON in memory
        var json =
            @"
        {
          ""Theme"": {
            ""Header"": {
              ""PanelHeader"": { ""Text"": ""HeaderText"" }
            },
            ""TypingArea"": {
              ""PanelHeader"": { ""Text"": ""TypingText"" }
            }
          }
        }";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act: bind into our strongly-typed ThemeSettings
        var themeSettings = configuration.GetSection("Theme").Get<RuntimeTheme>();

        // Assert: dictionary has LayoutName keys, not strings
        await Assert.That(themeSettings).IsNotNull();
        await Assert.That(themeSettings!.ContainsKey(LayoutName.Header)).IsTrue();
        await Assert.That(themeSettings!.ContainsKey(LayoutName.TypingArea)).IsTrue();

        // And check values came through
        await Assert
            .That(themeSettings[LayoutName.Header].PanelHeader.Text)
            .IsEqualTo("HeaderText");
        await Assert
            .That(themeSettings[LayoutName.TypingArea].PanelHeader.Text)
            .IsEqualTo("TypingText");
    }

    // [Test]
    // public async Task Debug_TypeConverter_For_LayoutName()
    // {
    //     var converter = TypeDescriptor.GetConverter(typeof(LayoutName));

    //     await Assert.That(converter).IsNotNull();
    //     await Assert.That(converter).IsTypeOf<LayoutNameTypeConverter>();

    //     // You can also test conversion directly:
    //     var layout = (LayoutName)converter.ConvertFrom("Header");
    //     await Assert.That(layout).IsEqualTo(LayoutName.Header);
    // }
}
