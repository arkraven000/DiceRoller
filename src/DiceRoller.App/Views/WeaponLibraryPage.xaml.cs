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
    public WeaponLibraryViewModel ViewModel { get; private set; }

    public WeaponLibraryPage()
    {
        InitializeComponent();
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
