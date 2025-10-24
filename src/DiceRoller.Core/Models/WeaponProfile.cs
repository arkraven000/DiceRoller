using DiceRoller.Core.Enums;

namespace DiceRoller.Core.Models;

/// <summary>
/// Represents a weapon profile in Warhammer 40K 10th Edition.
/// Contains all weapon characteristics needed for dice calculation.
/// </summary>
public class WeaponProfile
{
    /// <summary>
    /// Unique identifier for the weapon.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the weapon (e.g., "Bolter", "Lascannon").
    /// Maximum length: 100 characters (REQ-INPUT-003).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of attacks this weapon makes.
    /// Range: 1-1000 (REQ-INPUT-002).
    /// </summary>
    public int Attacks { get; set; }

    /// <summary>
    /// Ballistic Skill or Weapon Skill required to hit (2+ to 6+).
    /// Lower is better. Range: 2-6.
    /// </summary>
    public int BallisticSkill { get; set; }

    /// <summary>
    /// Strength characteristic of the weapon.
    /// Range: 1-20 (most weapons are 3-10).
    /// </summary>
    public int Strength { get; set; }

    /// <summary>
    /// Armor Penetration value (0 to -6).
    /// Negative values reduce the target's armor save.
    /// </summary>
    public int ArmorPenetration { get; set; }

    /// <summary>
    /// Damage characteristic type (Fixed, D3, D6, etc.).
    /// </summary>
    public DamageType DamageType { get; set; }

    /// <summary>
    /// Fixed damage value (if DamageType is Fixed).
    /// Range: 1-20.
    /// </summary>
    public int FixedDamage { get; set; }

    /// <summary>
    /// Special abilities this weapon has (flags).
    /// Can have multiple abilities combined.
    /// </summary>
    public WeaponAbility Abilities { get; set; }

    /// <summary>
    /// Anti-X threshold (for Anti-Infantry, Anti-Vehicle, etc.).
    /// Critical wounds occur on this value or higher (e.g., 4+ means 4, 5, 6).
    /// Range: 2-6, or 0 if not applicable.
    /// </summary>
    public int AntiThreshold { get; set; }

    /// <summary>
    /// Optional description or notes about the weapon.
    /// Maximum length: 500 characters (REQ-INPUT-003).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp when this weapon profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this weapon profile was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the weapon profile for correctness.
    /// Implements REQ-INPUT-002 validation.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            errorMessage = "Weapon name cannot be empty.";
            return false;
        }

        if (Name.Length > 100)
        {
            errorMessage = "Weapon name too long (max 100 characters).";
            return false;
        }

        if (Attacks < 1 || Attacks > 1000)
        {
            errorMessage = "Attacks must be between 1 and 1000.";
            return false;
        }

        if (BallisticSkill < 2 || BallisticSkill > 6)
        {
            errorMessage = "Ballistic Skill must be between 2+ and 6+.";
            return false;
        }

        if (Strength < 1 || Strength > 20)
        {
            errorMessage = "Strength must be between 1 and 20.";
            return false;
        }

        if (ArmorPenetration < -6 || ArmorPenetration > 0)
        {
            errorMessage = "Armor Penetration must be between 0 and -6.";
            return false;
        }

        if (DamageType == DamageType.Fixed && (FixedDamage < 1 || FixedDamage > 20))
        {
            errorMessage = "Fixed damage must be between 1 and 20.";
            return false;
        }

        if (AntiThreshold != 0 && (AntiThreshold < 2 || AntiThreshold > 6))
        {
            errorMessage = "Anti threshold must be 0 or between 2 and 6.";
            return false;
        }

        if (Description != null && Description.Length > 500)
        {
            errorMessage = "Description too long (max 500 characters).";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Creates a copy of this weapon profile.
    /// </summary>
    public WeaponProfile Clone()
    {
        return new WeaponProfile
        {
            Id = 0, // New copy gets new ID
            Name = $"{Name} (Copy)",
            Attacks = Attacks,
            BallisticSkill = BallisticSkill,
            Strength = Strength,
            ArmorPenetration = ArmorPenetration,
            DamageType = DamageType,
            FixedDamage = FixedDamage,
            Abilities = Abilities,
            AntiThreshold = AntiThreshold,
            Description = Description,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}
