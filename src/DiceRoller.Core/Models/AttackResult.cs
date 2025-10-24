namespace DiceRoller.Core.Models;

/// <summary>
/// Represents the calculated results of an attack sequence.
/// Contains probabilities and expected outcomes.
/// </summary>
public class AttackResult
{
    /// <summary>
    /// Expected number of successful hits.
    /// </summary>
    public double ExpectedHits { get; set; }

    /// <summary>
    /// Expected number of successful wounds.
    /// </summary>
    public double ExpectedWounds { get; set; }

    /// <summary>
    /// Expected number of failed saves.
    /// </summary>
    public double ExpectedFailedSaves { get; set; }

    /// <summary>
    /// Expected total damage inflicted (including mortal wounds).
    /// </summary>
    public double ExpectedDamage { get; set; }

    /// <summary>
    /// Expected number of mortal wounds inflicted.
    /// </summary>
    public double ExpectedMortalWounds { get; set; }

    /// <summary>
    /// Expected number of models killed.
    /// </summary>
    public double ExpectedModelsKilled { get; set; }

    /// <summary>
    /// Probability of killing at least one model (0.0 to 1.0).
    /// </summary>
    public double ProbabilityOfKill { get; set; }

    /// <summary>
    /// Probability of wiping out the entire unit (0.0 to 1.0).
    /// </summary>
    public double ProbabilityOfWipe { get; set; }

    /// <summary>
    /// Number of critical hits generated.
    /// </summary>
    public double ExpectedCriticalHits { get; set; }

    /// <summary>
    /// Number of critical wounds generated.
    /// </summary>
    public double ExpectedCriticalWounds { get; set; }

    /// <summary>
    /// Detailed breakdown of calculations (optional, for display).
    /// </summary>
    public string? CalculationBreakdown { get; set; }

    /// <summary>
    /// Creates a zero-result (no damage).
    /// </summary>
    public static AttackResult Zero()
    {
        return new AttackResult
        {
            ExpectedHits = 0,
            ExpectedWounds = 0,
            ExpectedFailedSaves = 0,
            ExpectedDamage = 0,
            ExpectedMortalWounds = 0,
            ExpectedModelsKilled = 0,
            ProbabilityOfKill = 0,
            ProbabilityOfWipe = 0,
            ExpectedCriticalHits = 0,
            ExpectedCriticalWounds = 0
        };
    }
}

/// <summary>
/// Represents a single simulated dice roll outcome.
/// Used for Monte Carlo simulation mode.
/// </summary>
public class SimulatedAttack
{
    /// <summary>
    /// Number of hits in this simulation.
    /// </summary>
    public int Hits { get; set; }

    /// <summary>
    /// Number of wounds in this simulation.
    /// </summary>
    public int Wounds { get; set; }

    /// <summary>
    /// Number of failed saves in this simulation.
    /// </summary>
    public int FailedSaves { get; set; }

    /// <summary>
    /// Total damage inflicted in this simulation.
    /// </summary>
    public int TotalDamage { get; set; }

    /// <summary>
    /// Mortal wounds inflicted in this simulation.
    /// </summary>
    public int MortalWounds { get; set; }

    /// <summary>
    /// Number of models killed in this simulation.
    /// </summary>
    public int ModelsKilled { get; set; }
}

/// <summary>
/// Results from a Monte Carlo simulation of multiple dice roll iterations.
/// </summary>
public class SimulationResult
{
    /// <summary>
    /// Number of iterations performed.
    /// </summary>
    public int Iterations { get; set; }

    /// <summary>
    /// All simulated attacks.
    /// </summary>
    public List<SimulatedAttack> Simulations { get; set; } = new();

    /// <summary>
    /// Average damage across all simulations.
    /// </summary>
    public double AverageDamage { get; set; }

    /// <summary>
    /// Median damage value.
    /// </summary>
    public double MedianDamage { get; set; }

    /// <summary>
    /// Minimum damage observed.
    /// </summary>
    public int MinimumDamage { get; set; }

    /// <summary>
    /// Maximum damage observed.
    /// </summary>
    public int MaximumDamage { get; set; }

    /// <summary>
    /// Standard deviation of damage.
    /// </summary>
    public double StandardDeviation { get; set; }

    /// <summary>
    /// Histogram data: damage value → frequency count.
    /// </summary>
    public Dictionary<int, int> DamageHistogram { get; set; } = new();

    /// <summary>
    /// Percentage of simulations that killed at least one model.
    /// </summary>
    public double PercentageWithKills { get; set; }

    /// <summary>
    /// Percentage of simulations that wiped the unit.
    /// </summary>
    public double PercentageWipeout { get; set; }
}
