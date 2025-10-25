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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
