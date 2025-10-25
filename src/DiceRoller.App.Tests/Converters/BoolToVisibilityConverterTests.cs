using DiceRoller.App.Converters;
using FluentAssertions;
using Microsoft.UI.Xaml;
using Xunit;

namespace DiceRoller.App.Tests.Converters;

/// <summary>
/// Unit tests for BoolToVisibilityConverter.
/// Tests conversion of boolean values to Visibility enum values.
/// </summary>
public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter;

    public BoolToVisibilityConverterTests()
    {
        _converter = new BoolToVisibilityConverter();
    }

    [Fact]
    public void Convert_WithTrue_ShouldReturnVisible()
    {
        // Arrange
        bool value = true;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Visible, "true should convert to Visible");
    }

    [Fact]
    public void Convert_WithFalse_ShouldReturnCollapsed()
    {
        // Arrange
        bool value = false;

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "false should convert to Collapsed");
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
    public void Convert_WithNonBooleanValue_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "not a boolean";

        // Act
        object result = _converter.Convert(value, typeof(Visibility), null, string.Empty);

        // Assert
        result.Should().Be(Visibility.Collapsed, "non-boolean values should convert to Collapsed");
    }

    [Fact]
    public void ConvertBack_WithVisible_ShouldReturnTrue()
    {
        // Arrange
        Visibility value = Visibility.Visible;

        // Act
        object result = _converter.ConvertBack(value, typeof(bool), null, string.Empty);

        // Assert
        result.Should().Be(true, "Visible should convert back to true");
    }

    [Fact]
    public void ConvertBack_WithCollapsed_ShouldReturnFalse()
    {
        // Arrange
        Visibility value = Visibility.Collapsed;

        // Act
        object result = _converter.ConvertBack(value, typeof(bool), null, string.Empty);

        // Assert
        result.Should().Be(false, "Collapsed should convert back to false");
    }

    [Fact]
    public void RoundTrip_TrueToVisibleToTrue_ShouldPreserveValue()
    {
        // Arrange
        bool originalValue = true;

        // Act
        object visibility = _converter.Convert(originalValue, typeof(Visibility), null, string.Empty);
        object roundTrip = _converter.ConvertBack(visibility, typeof(bool), null, string.Empty);

        // Assert
        roundTrip.Should().Be(originalValue, "round trip should preserve original value");
    }

    [Fact]
    public void RoundTrip_FalseToCollapsedToFalse_ShouldPreserveValue()
    {
        // Arrange
        bool originalValue = false;

        // Act
        object visibility = _converter.Convert(originalValue, typeof(Visibility), null, string.Empty);
        object roundTrip = _converter.ConvertBack(visibility, typeof(bool), null, string.Empty);

        // Assert
        roundTrip.Should().Be(originalValue, "round trip should preserve original value");
    }
}
