using System.Security.Cryptography;
using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;

namespace DiceRoller.Core.Services.Calculation;

/// <summary>
/// Implements dice probability calculations for Warhammer 40K 10th Edition.
/// Uses cryptographically secure random number generation (REQ-CRYPTO-003).
/// </summary>
public class DiceCalculator : IDiceCalculator, IDisposable
{
    private readonly RandomNumberGenerator _rng;
    private bool _disposed;

    public DiceCalculator()
    {
        // REQ-CRYPTO-003: Use cryptographically secure RNG
        _rng = RandomNumberGenerator.Create();
    }

    /// <summary>
    /// Calculates expected outcomes using probability math.
    /// </summary>
    public AttackResult CalculateAttack(
        WeaponProfile weapon,
        UnitProfile target,
        AttackModifiers modifiers)
    {
        // Validate inputs (REQ-INPUT-001)
        if (!weapon.IsValid(out string? weaponError))
        {
            throw new ArgumentException($"Invalid weapon profile: {weaponError}", nameof(weapon));
        }

        if (!target.IsValid(out string? targetError))
        {
            throw new ArgumentException($"Invalid target profile: {targetError}", nameof(target));
        }

        if (!modifiers.IsValid(out string? modifierError))
        {
            throw new ArgumentException($"Invalid modifiers: {modifierError}", nameof(modifiers));
        }

        // Calculate number of attacks (with Blast, Rapid Fire, etc.)
        double totalAttacks = CalculateTotalAttacks(weapon, modifiers);

        // Step 1: Calculate hits
        var hitResult = CalculateHits(weapon, totalAttacks, modifiers);

        // Step 2: Calculate wounds
        var woundResult = CalculateWounds(weapon, target, hitResult, modifiers);

        // Step 3: Calculate saves
        var saveResult = CalculateSaves(weapon, target, woundResult, modifiers);

        // Step 4: Calculate damage
        var damageResult = CalculateDamage(weapon, target, saveResult, modifiers);

        return damageResult;
    }

    /// <summary>
    /// Runs Monte Carlo simulation using actual dice rolls.
    /// </summary>
    public SimulationResult RunSimulation(
        WeaponProfile weapon,
        UnitProfile target,
        AttackModifiers modifiers,
        int iterations = 10000)
    {
        // Validate iteration count (REQ-INPUT-002)
        if (iterations < 1 || iterations > 1000000)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations),
                "Iterations must be between 1 and 1,000,000.");
        }

        var simulations = new List<SimulatedAttack>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            var result = SimulateSingleAttack(weapon, target, modifiers);
            simulations.Add(result);
        }

        return AnalyzeSimulations(simulations, target);
    }

    /// <summary>
    /// Calculates total number of attacks (including Blast, Rapid Fire, etc.).
    /// </summary>
    private double CalculateTotalAttacks(WeaponProfile weapon, AttackModifiers modifiers)
    {
        double attacks = weapon.Attacks;

        // Blast: Add 1 attack per 5 models
        if (weapon.Abilities.HasFlag(WeaponAbility.Blast))
        {
            int bonusAttacks = modifiers.TargetUnitSize / 5;
            attacks += bonusAttacks;
        }

        // Rapid Fire: Double attacks at half range
        if (modifiers.WithinHalfRange)
        {
            if (weapon.Abilities.HasFlag(WeaponAbility.RapidFire1))
            {
                attacks += 1;
            }
            else if (weapon.Abilities.HasFlag(WeaponAbility.RapidFire2))
            {
                attacks += 2;
            }
        }

        return attacks;
    }

    /// <summary>
    /// Calculates expected hits based on BS and modifiers.
    /// </summary>
    private HitCalculation CalculateHits(
        WeaponProfile weapon,
        double totalAttacks,
        AttackModifiers modifiers)
    {
        // Torrent auto-hits
        if (weapon.Abilities.HasFlag(WeaponAbility.Torrent))
        {
            return new HitCalculation
            {
                TotalHits = totalAttacks,
                CriticalHits = 0,
                NormalHits = totalAttacks
            };
        }

        // Calculate hit probability with modifier cap (REQ: modifier cap +1/-1)
        int effectiveBS = weapon.BallisticSkill + Math.Clamp(modifiers.HitModifier, -1, 1);
        effectiveBS = Math.Clamp(effectiveBS, 2, 6); // BS cannot go below 2+ or above 6+

        double hitChance = CalculateRollProbability(effectiveBS, modifiers.HitReroll);
        double critChance = CalculateCriticalProbability(modifiers.CriticalHitThreshold, modifiers.HitReroll);

        double normalHits = totalAttacks * (hitChance - critChance);
        double criticalHits = totalAttacks * critChance;

        // Sustained Hits: Critical hits generate additional hits
        if (weapon.Abilities.HasFlag(WeaponAbility.SustainedHits1))
        {
            normalHits += criticalHits * 1; // +1 additional hit per crit
        }
        else if (weapon.Abilities.HasFlag(WeaponAbility.SustainedHits2))
        {
            normalHits += criticalHits * 2; // +2 additional hits per crit
        }
        else if (weapon.Abilities.HasFlag(WeaponAbility.SustainedHits3))
        {
            normalHits += criticalHits * 3; // +3 additional hits per crit
        }

        return new HitCalculation
        {
            TotalHits = normalHits + criticalHits,
            CriticalHits = criticalHits,
            NormalHits = normalHits
        };
    }

    /// <summary>
    /// Calculates expected wounds based on S vs T comparison.
    /// </summary>
    private WoundCalculation CalculateWounds(
        WeaponProfile weapon,
        UnitProfile target,
        HitCalculation hits,
        AttackModifiers modifiers)
    {
        // Lethal Hits: Critical hits auto-wound
        double autoWounds = 0;
        double hitsToRoll = hits.NormalHits;

        if (weapon.Abilities.HasFlag(WeaponAbility.LethalHits))
        {
            autoWounds = hits.CriticalHits; // Crits become automatic wounds
        }
        else
        {
            hitsToRoll += hits.CriticalHits; // Crits still need to wound normally
        }

        // Determine wound threshold based on S vs T
        int woundThreshold = GetWoundThreshold(weapon.Strength, target.Toughness);

        // Apply wound modifier (capped at +1/-1)
        int effectiveThreshold = woundThreshold + Math.Clamp(modifiers.WoundModifier, -1, 1);
        effectiveThreshold = Math.Clamp(effectiveThreshold, 2, 6);

        // Apply Twin-Linked (re-roll wound rolls)
        RerollType effectiveWoundReroll = modifiers.WoundReroll;
        if (weapon.Abilities.HasFlag(WeaponAbility.TwinLinked))
        {
            effectiveWoundReroll = RerollType.RerollFailed;
        }

        // Apply Anti-X abilities - lower critical wound threshold if weapon type matches target keywords
        int effectiveCritWoundThreshold = modifiers.CriticalWoundThreshold;

        if (weapon.AntiThreshold > 0)
        {
            if (weapon.Abilities.HasFlag(WeaponAbility.AntiInfantry) &&
                target.Keywords.HasFlag(UnitKeyword.Infantry))
            {
                effectiveCritWoundThreshold = Math.Min(effectiveCritWoundThreshold, weapon.AntiThreshold);
            }

            if (weapon.Abilities.HasFlag(WeaponAbility.AntiVehicle) &&
                target.Keywords.HasFlag(UnitKeyword.Vehicle))
            {
                effectiveCritWoundThreshold = Math.Min(effectiveCritWoundThreshold, weapon.AntiThreshold);
            }

            if (weapon.Abilities.HasFlag(WeaponAbility.AntiMonster) &&
                target.Keywords.HasFlag(UnitKeyword.Monster))
            {
                effectiveCritWoundThreshold = Math.Min(effectiveCritWoundThreshold, weapon.AntiThreshold);
            }
        }

        double woundChance = CalculateRollProbability(effectiveThreshold, effectiveWoundReroll);
        double critWoundChance = CalculateCriticalProbability(effectiveCritWoundThreshold, effectiveWoundReroll);

        double normalWounds = hitsToRoll * (woundChance - critWoundChance);
        double criticalWounds = hitsToRoll * critWoundChance;

        return new WoundCalculation
        {
            TotalWounds = normalWounds + criticalWounds + autoWounds,
            CriticalWounds = criticalWounds,
            NormalWounds = normalWounds + autoWounds
        };
    }

    /// <summary>
    /// Calculates expected failed saves.
    /// </summary>
    private SaveCalculation CalculateSaves(
        WeaponProfile weapon,
        UnitProfile target,
        WoundCalculation wounds,
        AttackModifiers modifiers)
    {
        double mortalWounds = 0;
        double woundsToSave = wounds.NormalWounds;

        // Devastating Wounds: Critical wounds become mortal wounds
        if (weapon.Abilities.HasFlag(WeaponAbility.DevastatingWounds))
        {
            // Mortal wounds = critical wounds × damage
            double avgDamage = GetAverageDamage(weapon, modifiers);
            mortalWounds = wounds.CriticalWounds * avgDamage;
        }
        else
        {
            woundsToSave += wounds.CriticalWounds;
        }

        // Calculate effective save (armor save modified by AP, or invuln if better)
        int armorSave = target.Save - weapon.ArmorPenetration;
        int effectiveSave = target.InvulnerableSave > 0 && target.InvulnerableSave < armorSave
            ? target.InvulnerableSave
            : armorSave;

        // Apply cover bonus if applicable
        if (target.InCover && !weapon.Abilities.HasFlag(WeaponAbility.IgnoresCover))
        {
            effectiveSave -= 1; // Cover gives +1 to save
        }

        // Apply save modifier (capped)
        effectiveSave += Math.Clamp(modifiers.SaveModifier, -1, 1);
        effectiveSave = Math.Clamp(effectiveSave, 2, 7); // 7+ means no save

        double saveChance = effectiveSave >= 7 ? 0 : (7 - effectiveSave) / 6.0;
        double failedSaves = woundsToSave * (1 - saveChance);

        return new SaveCalculation
        {
            FailedSaves = failedSaves,
            MortalWounds = mortalWounds
        };
    }

    /// <summary>
    /// Calculates expected damage and models killed.
    /// </summary>
    private AttackResult CalculateDamage(
        WeaponProfile weapon,
        UnitProfile target,
        SaveCalculation saves,
        AttackModifiers modifiers)
    {
        double avgDamage = GetAverageDamage(weapon, modifiers);

        // Normal damage from failed saves
        double normalDamage = saves.FailedSaves * avgDamage;

        // Apply Feel No Pain
        if (target.FeelNoPain > 0)
        {
            double fnpChance = (7 - target.FeelNoPain) / 6.0;
            normalDamage *= (1 - fnpChance);
            saves.MortalWounds *= (1 - fnpChance);
        }

        double totalDamage = normalDamage + saves.MortalWounds;

        // Calculate models killed
        double modelsKilled = Math.Min(
            totalDamage / target.WoundsPerModel,
            target.ModelCount);

        return new AttackResult
        {
            ExpectedDamage = totalDamage,
            ExpectedMortalWounds = saves.MortalWounds,
            ExpectedFailedSaves = saves.FailedSaves,
            ExpectedModelsKilled = modelsKilled,
            ProbabilityOfKill = modelsKilled > 0 ? 1.0 : 0.0,
            ProbabilityOfWipe = modelsKilled >= target.ModelCount ? 1.0 : 0.0
        };
    }

    /// <summary>
    /// Determines wound threshold based on S vs T comparison (10th edition rules).
    /// </summary>
    private int GetWoundThreshold(int strength, int toughness)
    {
        if (strength >= toughness * 2) return 2; // S ≥ 2×T: 2+
        if (strength > toughness) return 3;      // S > T: 3+
        if (strength == toughness) return 4;     // S = T: 4+
        if (strength * 2 <= toughness) return 6; // S ≤ T/2: 6+
        return 5;                                 // S < T: 5+
    }

    /// <summary>
    /// Calculates average damage for a weapon.
    /// </summary>
    private double GetAverageDamage(WeaponProfile weapon, AttackModifiers modifiers)
    {
        double baseDamage = weapon.DamageType switch
        {
            DamageType.Fixed => weapon.FixedDamage,
            DamageType.D3 => 2.0,           // Average of D3
            DamageType.D6 => 3.5,           // Average of D6
            DamageType.TwoD6 => 7.0,        // Average of 2D6
            DamageType.D6Plus1 => 4.5,      // D6 + 1
            DamageType.D6Plus2 => 5.5,      // D6 + 2
            DamageType.TwoD6Plus3 => 10.0,  // 2D6 + 3
            DamageType.D3Plus3 => 5.0,      // D3 + 3
            _ => 1.0
        };

        // Melta bonus at half range
        if (modifiers.WithinHalfRange)
        {
            if (weapon.Abilities.HasFlag(WeaponAbility.Melta2))
            {
                baseDamage += 2;
            }
            else if (weapon.Abilities.HasFlag(WeaponAbility.Melta4))
            {
                baseDamage += 4;
            }
        }

        return baseDamage;
    }

    /// <summary>
    /// Calculates probability of rolling X+ on a D6 with re-rolls.
    /// </summary>
    private double CalculateRollProbability(int target, RerollType reroll)
    {
        if (target > 6) return 0;
        if (target < 2) return 1;

        double baseChance = (7 - target) / 6.0;

        return reroll switch
        {
            RerollType.None => baseChance,
            RerollType.RerollOnes => baseChance + (1.0 / 6.0) * baseChance,
            RerollType.RerollFailed => 1.0 - Math.Pow(1.0 - baseChance, 2),
            RerollType.RerollAll => 1.0 - Math.Pow(1.0 - baseChance, 2),
            _ => baseChance
        };
    }

    /// <summary>
    /// Calculates probability of rolling a critical (threshold+ on unmodified roll).
    /// </summary>
    private double CalculateCriticalProbability(int threshold, RerollType reroll)
    {
        double critChance = (7 - threshold) / 6.0;

        return reroll switch
        {
            RerollType.None => critChance,
            RerollType.RerollOnes => critChance + (1.0 / 6.0) * critChance,
            RerollType.RerollFailed => 1.0 - Math.Pow(1.0 - critChance, 2),
            RerollType.RerollAll => 1.0 - Math.Pow(1.0 - critChance, 2),
            _ => critChance
        };
    }

    /// <summary>
    /// Simulates a single attack sequence with actual dice rolls.
    /// REQ-CRYPTO-003: Uses cryptographically secure RNG.
    /// </summary>
    private SimulatedAttack SimulateSingleAttack(
        WeaponProfile weapon,
        UnitProfile target,
        AttackModifiers modifiers)
    {
        int totalAttacks = (int)Math.Round(CalculateTotalAttacks(weapon, modifiers));
        int hits = 0, wounds = 0, failedSaves = 0, totalDamage = 0, mortalWounds = 0;

        // Roll to hit
        for (int i = 0; i < totalAttacks; i++)
        {
            if (weapon.Abilities.HasFlag(WeaponAbility.Torrent) || RollD6() >= weapon.BallisticSkill)
            {
                hits++;
            }
        }

        // Roll to wound
        int woundThreshold = GetWoundThreshold(weapon.Strength, target.Toughness);
        for (int i = 0; i < hits; i++)
        {
            if (RollD6() >= woundThreshold)
            {
                wounds++;
            }
        }

        // Roll saves
        int effectiveSave = Math.Min(target.Save - weapon.ArmorPenetration, 7);
        for (int i = 0; i < wounds; i++)
        {
            if (RollD6() < effectiveSave)
            {
                failedSaves++;
            }
        }

        // Calculate damage
        for (int i = 0; i < failedSaves; i++)
        {
            totalDamage += RollDamage(weapon.DamageType, weapon.FixedDamage);
        }

        // Calculate models killed
        int modelsKilled = Math.Min(totalDamage / target.WoundsPerModel, target.ModelCount);

        return new SimulatedAttack
        {
            Hits = hits,
            Wounds = wounds,
            FailedSaves = failedSaves,
            TotalDamage = totalDamage,
            MortalWounds = mortalWounds,
            ModelsKilled = modelsKilled
        };
    }

    /// <summary>
    /// Rolls a D6 using cryptographically secure RNG.
    /// REQ-CRYPTO-003: Cryptographically secure random number generation.
    /// Uses rejection sampling to avoid modulo bias.
    /// </summary>
    private int RollD6()
    {
        byte[] randomBytes = new byte[4];
        // Use rejection sampling to avoid modulo bias
        // uint.MaxValue % 6 = 1, so we reject values >= (uint.MaxValue - 1)
        uint maxValue = uint.MaxValue - (uint.MaxValue % 6);

        while (true)
        {
            _rng.GetBytes(randomBytes);
            uint randomValue = BitConverter.ToUInt32(randomBytes, 0);

            if (randomValue < maxValue)
            {
                return (int)(randomValue % 6) + 1;
            }
            // Retry if value is in biased range (extremely rare)
        }
    }

    /// <summary>
    /// Rolls damage based on damage type.
    /// </summary>
    private int RollDamage(DamageType damageType, int fixedDamage)
    {
        return damageType switch
        {
            DamageType.Fixed => fixedDamage,
            DamageType.D3 => ((RollD6() - 1) / 2) + 1, // Maps 1-2→1, 3-4→2, 5-6→3
            DamageType.D6 => RollD6(),
            DamageType.TwoD6 => RollD6() + RollD6(),
            DamageType.D6Plus1 => RollD6() + 1,
            DamageType.D6Plus2 => RollD6() + 2,
            DamageType.TwoD6Plus3 => RollD6() + RollD6() + 3,
            DamageType.D3Plus3 => (((RollD6() - 1) / 2) + 1) + 3,
            _ => 1
        };
    }

    /// <summary>
    /// Analyzes simulation results to produce statistics.
    /// </summary>
    private SimulationResult AnalyzeSimulations(List<SimulatedAttack> simulations, UnitProfile target)
    {
        var damages = simulations.Select(s => s.TotalDamage).OrderBy(d => d).ToList();

        var result = new SimulationResult
        {
            Iterations = simulations.Count,
            Simulations = simulations,
            AverageDamage = damages.Average(),
            MedianDamage = damages.Count % 2 == 0
                ? (damages[damages.Count / 2 - 1] + damages[damages.Count / 2]) / 2.0
                : damages[damages.Count / 2],
            MinimumDamage = damages.Min(),
            MaximumDamage = damages.Max(),
            PercentageWithKills = simulations.Count(s => s.ModelsKilled > 0) * 100.0 / simulations.Count,
            PercentageWipeout = simulations.Count(s => s.ModelsKilled >= target.ModelCount) * 100.0 / simulations.Count
        };

        // Calculate standard deviation
        double variance = damages.Select(d => Math.Pow(d - result.AverageDamage, 2)).Average();
        result.StandardDeviation = Math.Sqrt(variance);

        // Build histogram
        result.DamageHistogram = damages.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());

        return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _rng?.Dispose();
            _disposed = true;
        }
    }
}

// Helper classes for intermediate calculations
internal class HitCalculation
{
    public double TotalHits { get; set; }
    public double CriticalHits { get; set; }
    public double NormalHits { get; set; }
}

internal class WoundCalculation
{
    public double TotalWounds { get; set; }
    public double CriticalWounds { get; set; }
    public double NormalWounds { get; set; }
}

internal class SaveCalculation
{
    public double FailedSaves { get; set; }
    public double MortalWounds { get; set; }
}
