using DiceRoller.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace DiceRoller.App.Views;

/// <summary>
/// Weapon library page for managing weapon profiles.
/// </summary>
public sealed partial class WeaponLibraryPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// Note: Initialized in OnNavigatedTo from DI container.
    /// The null-forgiving operator (null!) is used because x:Bind requires non-nullable properties,
    /// but initialization happens in the navigation lifecycle method.
    /// </summary>
    public WeaponLibraryViewModel ViewModel { get; private set; }

    public WeaponLibraryPage()
    {
        InitializeComponent();
        // Using null! because x:Bind requires non-nullable properties
        ViewModel = null!;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is IServiceProvider serviceProvider)
        {
            ViewModel = serviceProvider.GetRequiredService<WeaponLibraryViewModel>();
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
