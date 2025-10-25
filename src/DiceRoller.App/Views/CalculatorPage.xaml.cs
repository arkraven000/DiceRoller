using DiceRoller.App.ViewModels;
using DiceRoller.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace DiceRoller.App.Views;

/// <summary>
/// Main dice calculator page.
/// Implements grimdark Warhammer 40K themed combat analysis interface.
/// </summary>
public sealed partial class CalculatorPage : Page
{
    public CalculatorViewModel ViewModel { get; private set; }

    public CalculatorPage()
    {
        InitializeComponent();

        // ViewModel will be set in OnNavigatedTo after DI container is available
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is IServiceProvider serviceProvider)
        {
            // Get ViewModel from DI container
            ViewModel = serviceProvider.GetRequiredService<CalculatorViewModel>();

            // Initialize weapon and target with default values
            InitializeDefaults();

            // Set up event handlers for combo boxes
            SetupEventHandlers();
        }
    }

    /// <summary>
    /// Initializes default values for weapon and target profiles.
    /// Provides example values for "Bolter vs Space Marine" scenario.
    /// </summary>
    private void InitializeDefaults()
    {
        // Default Bolter profile
        ViewModel.Weapon.Name = "Bolter";
        ViewModel.Weapon.Attacks = 2;
        ViewModel.Weapon.BallisticSkill = 3;
        ViewModel.Weapon.Strength = 4;
        ViewModel.Weapon.ArmorPenetration = 0;
        ViewModel.Weapon.DamageType = DamageType.Fixed;
        ViewModel.Weapon.FixedDamage = 1;
        ViewModel.Weapon.Abilities = WeaponAbility.None;
        ViewModel.Weapon.AntiThreshold = 0;

        // Default Space Marine target
        ViewModel.Target.Name = "Space Marine";
        ViewModel.Target.Toughness = 4;
        ViewModel.Target.Save = 3;
        ViewModel.Target.InvulnerableSave = 0;
        ViewModel.Target.FeelNoPain = 0;
        ViewModel.Target.WoundsPerModel = 2;
        ViewModel.Target.ModelCount = 5;
        ViewModel.Target.Keywords = UnitKeyword.Infantry;
        ViewModel.Target.InCover = false;

        // Default modifiers
        ViewModel.Modifiers.HitModifier = 0;
        ViewModel.Modifiers.WoundModifier = 0;
        ViewModel.Modifiers.SaveModifier = 0;
        ViewModel.Modifiers.HitReroll = RerollType.None;
        ViewModel.Modifiers.WoundReroll = RerollType.None;
        ViewModel.Modifiers.SaveReroll = RerollType.None;
    }

    /// <summary>
    /// Sets up event handlers for ComboBox controls.
    /// Maps ComboBox selections to enum values.
    /// </summary>
    private void SetupEventHandlers()
    {
        // Damage Type ComboBox
        DamageTypeComboBox.SelectionChanged += (s, e) =>
        {
            ViewModel.Weapon.DamageType = DamageTypeComboBox.SelectedIndex switch
            {
                0 => DamageType.Fixed,
                1 => DamageType.D3,
                2 => DamageType.D6,
                3 => DamageType.TwoD6,
                4 => DamageType.D6Plus1,
                5 => DamageType.D6Plus2,
                _ => DamageType.Fixed
            };

            // Show/hide fixed damage input
            FixedDamageNumberBox.Visibility =
                DamageTypeComboBox.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        };

        // Hit Reroll ComboBox
        HitRerollComboBox.SelectionChanged += (s, e) =>
        {
            ViewModel.Modifiers.HitReroll = HitRerollComboBox.SelectedIndex switch
            {
                0 => RerollType.None,
                1 => RerollType.RerollOnes,
                2 => RerollType.RerollFailed,
                3 => RerollType.RerollAll,
                _ => RerollType.None
            };
        };

        // Wound Reroll ComboBox
        WoundRerollComboBox.SelectionChanged += (s, e) =>
        {
            ViewModel.Modifiers.WoundReroll = WoundRerollComboBox.SelectedIndex switch
            {
                0 => RerollType.None,
                1 => RerollType.RerollOnes,
                2 => RerollType.RerollFailed,
                3 => RerollType.RerollAll,
                _ => RerollType.None
            };
        };

        // Save Reroll ComboBox
        SaveRerollComboBox.SelectionChanged += (s, e) =>
        {
            ViewModel.Modifiers.SaveReroll = SaveRerollComboBox.SelectedIndex switch
            {
                0 => RerollType.None,
                1 => RerollType.RerollOnes,
                2 => RerollType.RerollFailed,
                3 => RerollType.RerollAll,
                _ => RerollType.None
            };
        };

        // Weapon Abilities CheckBoxes
        // Note: Using FindName to get checkboxes by name since we can't use x:Name with x:Bind
        // Will need to update this based on actual checkbox names in XAML
    }
}
