using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DiceRoller.App.Converters;

/// <summary>
/// Converts string to Visibility.
/// Non-empty string = Visible, Empty/Null = Collapsed.
/// </summary>
public class StringVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// ConvertBack is not implemented because converting Visibility back to a string is ambiguous.
    /// Visibility.Collapsed could represent either null, empty string, or whitespace.
    /// This converter is intended for one-way binding only.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack is not supported for StringVisibilityConverter. Use one-way binding only.");
    }
}
