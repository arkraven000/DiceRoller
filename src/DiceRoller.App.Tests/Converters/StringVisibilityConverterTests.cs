using DiceRoller.App.Converters;
using FluentAssertions;
using Microsoft.UI.Xaml;
using Xunit;

namespace DiceRoller.App.Tests.Converters;

/// <summary>
/// Unit tests for StringVisibilityConverter.
/// Tests conversion of string values to Visibility based on content.
/// </summary>
public class StringVisibilityConverterTests
{
    private readonly StringVisibilityConverter _converter;

    public StringVisibilityConverterTests()
    {
        _converter = new StringVisibilityConverter();
    }

    [Fact]
    public void Convert_WithNull_ShouldReturnCollapsed()
    {
        // Act
        object result = _converter.Convert(null, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "null should convert to Collapsed");
    }

    [Fact]
    public void Convert_WithEmptyString_ShouldReturnCollapsed()
    {
        // Arrange
        string value = string.Empty;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "empty string should convert to Collapsed");
    }

    [Fact]
    public void Convert_WithWhitespace_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "   ";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "whitespace-only string should convert to Collapsed");
    }

    [Fact]
    public void Convert_WithValidString_ShouldReturnVisible()
    {
        // Arrange
        string value = "Hello, World!";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "non-empty string should convert to Visible");
    }

    [Fact]
    public void Convert_WithSingleCharacter_ShouldReturnVisible()
    {
        // Arrange
        string value = "A";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "single character string should convert to Visible");
    }

    [Fact]
    public void Convert_WithStringContainingOnlyNewline_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "\n";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "newline-only string should convert to Collapsed");
    }

    [Fact]
    public void Convert_WithStringContainingTabs_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "\t\t";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "tab-only string should convert to Collapsed");
    }

    [Fact]
    public void Convert_WithMixedWhitespaceAndText_ShouldReturnVisible()
    {
        // Arrange
        string value = "  text  ";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "string with content (even with whitespace) should convert to Visible");
    }

    [Fact]
    public void Convert_WithNonStringValue_ShouldReturnCollapsed()
    {
        // Arrange
        int value = 123;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "non-string value should convert to Collapsed");
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(string), null, string.Empty);
        act.Should().Throw<System.NotImplementedException>("ConvertBack is not supported for one-way binding");
    }
}
