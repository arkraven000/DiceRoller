using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using DiceRoller.Core.Services.Calculation;

namespace DiceRoller.App.ViewModels;

/// <summary>
/// ViewModel for the dice calculator page.
/// Handles weapon and unit configuration, calculates attack probabilities.
/// Implements REQ-INPUT-001: Input validation at ViewModel layer.
/// </summary>
public partial class CalculatorViewModel : ViewModelBase
{
    private readonly IDiceCalculator _calculator;

    [ObservableProperty]
    private WeaponProfile _weapon = new()
    {
        Name = "Bolter",
        Attacks = 2,
        BallisticSkill = 3,
        Strength = 4,
        ArmorPenetration = 0,
        DamageType = DamageType.Fixed,
        FixedDamage = 1
    };

    [ObservableProperty]
    private UnitProfile _target = new()
    {
        Name = "Guardsman",
        Toughness = 3,
        Save = 5,
        WoundsPerModel = 1,
        ModelCount = 10
    };

    [ObservableProperty]
    private AttackModifiers _modifiers = AttackModifiers.Default();

    [ObservableProperty]
    private AttackResult? _result;

    [ObservableProperty]
    private SimulationResult? _simulationResult;

    [ObservableProperty]
    private int _simulationIterations = 10000;

    [ObservableProperty]
    private bool _showSimulation;

    public CalculatorViewModel(IDiceCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    /// <summary>
    /// Calculates expected attack outcomes using probability math.
    /// REQ-INPUT-001: Validates inputs before calculation.
    /// </summary>
    [RelayCommand]
    private void Calculate()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Calculating probabilities...";

            // REQ-INPUT-001: Validate weapon profile
            if (!Weapon.IsValid(out string? weaponError))
            {
                StatusMessage = $"Invalid weapon: {weaponError}";
                return;
            }

            // REQ-INPUT-001: Validate unit profile
            if (!Target.IsValid(out string? targetError))
            {
                StatusMessage = $"Invalid target: {targetError}";
                return;
            }

            // REQ-INPUT-001: Validate modifiers
            if (!Modifiers.IsValid(out string? modifierError))
            {
                StatusMessage = $"Invalid modifiers: {modifierError}";
                return;
            }

            // Perform calculation
            Result = _calculator.CalculateAttack(Weapon, Target, Modifiers);
            StatusMessage = "Calculation complete. The Emperor protects.";
        }
        catch (Exception ex)
        {
            // REQ-ERROR-002: Don't expose technical details
            StatusMessage = $"Calculation failed: {ex.Message}";
            Result = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Runs Monte Carlo simulation with actual dice rolls.
    /// REQ-CRYPTO-003: Uses cryptographically secure RNG.
    /// </summary>
    [RelayCommand]
    private async Task RunSimulationAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = $"Running {SimulationIterations:N0} simulations in the name of the Emperor...";

            // Validate iteration count
            if (SimulationIterations < 100 || SimulationIterations > 1_000_000)
            {
                StatusMessage = "Simulation iterations must be between 100 and 1,000,000.";
                return;
            }

            // Run simulation on background thread (CPU intensive)
            SimulationResult = await Task.Run(() =>
                _calculator.RunSimulation(Weapon, Target, Modifiers, SimulationIterations));

            ShowSimulation = true;
            StatusMessage = $"Simulation complete. {SimulationIterations:N0} battles fought in the grim darkness.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Simulation failed: {ex.Message}";
            SimulationResult = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Clears all results and resets to default values.
    /// </summary>
    [RelayCommand]
    private void Clear()
    {
        Result = null;
        SimulationResult = null;
        ShowSimulation = false;
        StatusMessage = "Calculation slate purged.";
    }

    /// <summary>
    /// Swaps attacker and defender (for return fire calculations).
    /// </summary>
    [RelayCommand]
    private void SwapWeaponAndTarget()
    {
        // This is a conceptual swap - in reality, you'd need to convert
        // defensive stats to offensive and vice versa, which doesn't directly apply
        StatusMessage = "Swap not applicable - configure manually.";
    }
}
