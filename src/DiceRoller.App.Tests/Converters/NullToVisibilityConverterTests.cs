using DiceRoller.App.Converters;
using FluentAssertions;
using Microsoft.UI.Xaml;
using Xunit;

namespace DiceRoller.App.Tests.Converters;

/// <summary>
/// Unit tests for NullToVisibilityConverter.
/// Tests conversion of null/non-null values to Visibility.
/// </summary>
public class NullToVisibilityConverterTests
{
    private readonly NullToVisibilityConverter _converter;

    public NullToVisibilityConverterTests()
    {
        _converter = new NullToVisibilityConverter();
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
    public void Convert_WithNonNullObject_ShouldReturnVisible()
    {
        // Arrange
        object value = new object();

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "non-null should convert to Visible");
    }

    [Fact]
    public void Convert_WithNonNullString_ShouldReturnVisible()
    {
        // Arrange
        string value = "test";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "non-null string should convert to Visible");
    }

    [Fact]
    public void Convert_WithEmptyString_ShouldReturnVisible()
    {
        // Arrange
        string value = string.Empty;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "empty string is not null, should convert to Visible");
    }

    [Fact]
    public void Convert_WithZero_ShouldReturnVisible()
    {
        // Arrange
        int value = 0;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "zero is a valid value (not null), should convert to Visible");
    }

    [Fact]
    public void Convert_WithFalse_ShouldReturnVisible()
    {
        // Arrange
        bool value = false;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "false is a valid value (not null), should convert to Visible");
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(object), null, string.Empty);
        act.Should().Throw<System.NotImplementedException>("ConvertBack is not supported for one-way binding");
    }
}
