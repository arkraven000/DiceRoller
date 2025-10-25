using DiceRoller.App.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace DiceRoller.App;

/// <summary>
/// Main application window with navigation.
/// Implements grimdark Warhammer 40K theme navigation.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        InitializeComponent();

        // Set window properties
        Title = "WARHAMMER 40,000 DICE CALCULATOR - In the grim darkness of the far future, there is only war";

        // Navigate to calculator page by default
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        ContentFrame.Navigate(typeof(CalculatorPage), _serviceProvider);
    }

    /// <summary>
    /// Handles navigation view selection changes.
    /// Routes to appropriate page based on selection.
    /// </summary>
    private void NavigationView_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem)
        {
            string? tag = selectedItem.Tag?.ToString();

            Type? pageType = tag switch
            {
                "Calculator" => typeof(CalculatorPage),
                "WeaponLibrary" => typeof(WeaponLibraryPage),
                "UnitLibrary" => typeof(UnitLibraryPage),
                _ => null
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                // Update header
                NavigationViewControl.Header = selectedItem.Content;

                // Navigate to page
                ContentFrame.Navigate(pageType, _serviceProvider);
            }
        }
    }

    /// <summary>
    /// Shows the loading overlay during long operations.
    /// </summary>
    public void ShowLoadingOverlay()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the loading overlay.
    /// </summary>
    public void HideLoadingOverlay()
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
    }
}
