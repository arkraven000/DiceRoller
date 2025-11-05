using DiceRoller.Core.Enums;

namespace DiceRoller.Core.Models;

/// <summary>
/// Represents modifiers applied to an attack sequence.
/// Includes re-rolls, bonuses, and special conditions.
/// </summary>
public class AttackModifiers
{
    /// <summary>
    /// Hit roll modifier (-1, 0, or +1).
    /// Capped at +1/-1 per 10th edition rules.
    /// </summary>
    public int HitModifier { get; set; }

    /// <summary>
    /// Wound roll modifier (-1, 0, or +1).
    /// Capped at +1/-1 per 10th edition rules.
    /// </summary>
    public int WoundModifier { get; set; }

    /// <summary>
    /// Save roll modifier (-1, 0, or +1).
    /// Capped at +1/-1 per 10th edition rules.
    /// </summary>
    public int SaveModifier { get; set; }

    /// <summary>
    /// Re-roll type for hit rolls.
    /// </summary>
    public RerollType HitReroll { get; set; } = RerollType.None;

    /// <summary>
    /// Re-roll type for wound rolls.
    /// </summary>
    public RerollType WoundReroll { get; set; } = RerollType.None;

    /// <summary>
    /// Re-roll type for damage rolls.
    /// </summary>
    public RerollType DamageReroll { get; set; } = RerollType.None;

    /// <summary>
    /// Re-roll type for save rolls.
    /// </summary>
    public RerollType SaveReroll { get; set; } = RerollType.None;

    /// <summary>
    /// Whether the attacker is within half range (for Melta, Rapid Fire, etc.).
    /// </summary>
    public bool WithinHalfRange { get; set; }

    /// <summary>
    /// Target unit size (for Blast weapons).
    /// Range: 1-50.
    /// </summary>
    public int TargetUnitSize { get; set; } = 1;

    /// <summary>
    /// Critical hit threshold (normally 6, but can be modified).
    /// Range: 2-6.
    /// </summary>
    public int CriticalHitThreshold { get; set; } = 6;

    /// <summary>
    /// Critical wound threshold (normally 6, but can be modified by Anti-X abilities).
    /// Range: 2-6.
    /// </summary>
    public int CriticalWoundThreshold { get; set; } = 6;

    /// <summary>
    /// Validates the modifiers for correctness.
    /// Implements 10th edition modifier caps.
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        // 10th edition rule: modifiers capped at +1/-1
        if (HitModifier < -1 || HitModifier > 1)
        {
            errorMessage = "Hit modifier must be between -1 and +1.";
            return false;
        }

        if (WoundModifier < -1 || WoundModifier > 1)
        {
            errorMessage = "Wound modifier must be between -1 and +1.";
            return false;
        }

        if (SaveModifier < -1 || SaveModifier > 1)
        {
            errorMessage = "Save modifier must be between -1 and +1.";
            return false;
        }

        if (TargetUnitSize < 1 || TargetUnitSize > 50)
        {
            errorMessage = "Target unit size must be between 1 and 50.";
            return false;
        }

        if (CriticalHitThreshold < 2 || CriticalHitThreshold > 6)
        {
            errorMessage = "Critical hit threshold must be between 2 and 6.";
            return false;
        }

        if (CriticalWoundThreshold < 2 || CriticalWoundThreshold > 6)
        {
            errorMessage = "Critical wound threshold must be between 2 and 6.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Creates default modifiers (no modifiers applied).
    /// </summary>
    public static AttackModifiers Default()
    {
        return new AttackModifiers
        {
            HitModifier = 0,
            WoundModifier = 0,
            SaveModifier = 0,
            HitReroll = RerollType.None,
            WoundReroll = RerollType.None,
            DamageReroll = RerollType.None,
            SaveReroll = RerollType.None,
            WithinHalfRange = false,
            TargetUnitSize = 1,
            CriticalHitThreshold = 6,
            CriticalWoundThreshold = 6
        };
    }
}
