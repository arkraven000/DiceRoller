using DiceRoller.Core.Models;

namespace DiceRoller.Core.Services.Calculation;

/// <summary>
/// Interface for dice calculation services.
/// Calculates attack probabilities and expected outcomes for Warhammer 40K.
/// </summary>
public interface IDiceCalculator
{
    /// <summary>
    /// Calculates expected outcomes for an attack.
    /// </summary>
    /// <param name="weapon">The attacking weapon profile.</param>
    /// <param name="target">The defending unit profile.</param>
    /// <param name="modifiers">Additional modifiers to apply.</param>
    /// <returns>Calculated attack results with probabilities.</returns>
    AttackResult CalculateAttack(
        WeaponProfile weapon,
        UnitProfile target,
        AttackModifiers modifiers);

    /// <summary>
    /// Runs a Monte Carlo simulation of the attack.
    /// </summary>
    /// <param name="weapon">The attacking weapon profile.</param>
    /// <param name="target">The defending unit profile.</param>
    /// <param name="modifiers">Additional modifiers to apply.</param>
    /// <param name="iterations">Number of simulations to run (default 10,000).</param>
    /// <returns>Simulation results with statistics.</returns>
    SimulationResult RunSimulation(
        WeaponProfile weapon,
        UnitProfile target,
        AttackModifiers modifiers,
        int iterations = 10000);
}
