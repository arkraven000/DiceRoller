using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DiceRoller.App.Converters;

/// <summary>
/// Converts null to Visibility.
/// Not null = Visible, Null = Collapsed.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// ConvertBack is not implemented because converting Visibility back to an object is ambiguous.
    /// Visibility.Collapsed represents null, but Visibility.Visible could represent any non-null object.
    /// This converter is intended for one-way binding only.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack is not supported for NullToVisibilityConverter. Use one-way binding only.");
    }
}
