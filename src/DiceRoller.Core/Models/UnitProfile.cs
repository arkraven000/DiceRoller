using DiceRoller.Core.Enums;

namespace DiceRoller.Core.Models;

/// <summary>
/// Represents a unit profile (defender) in Warhammer 40K 10th Edition.
/// Contains defensive characteristics needed for dice calculation.
/// </summary>
public class UnitProfile
{
    /// <summary>
    /// Unique identifier for the unit.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the unit (e.g., "Space Marine", "Guardsman").
    /// Maximum length: 100 characters (REQ-INPUT-003).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Toughness characteristic of the unit.
    /// Range: 1-12 (most units are 3-8).
    /// </summary>
    public int Toughness { get; set; }

    /// <summary>
    /// Armor Save value (2+ to 6+, or 7+ for no save).
    /// Lower is better.
    /// </summary>
    public int Save { get; set; }

    /// <summary>
    /// Invulnerable Save value (0 for none, 2+ to 6+ if present).
    /// Cannot be modified by AP. 0 means no invulnerable save.
    /// </summary>
    public int InvulnerableSave { get; set; }

    /// <summary>
    /// Feel No Pain value (0 for none, 2+ to 6+ if present).
    /// Roll to ignore damage after failed save. 0 means no FNP.
    /// </summary>
    public int FeelNoPain { get; set; }

    /// <summary>
    /// Wounds per model.
    /// Range: 1-24 (most models are 1-3, vehicles/monsters 10-24).
    /// </summary>
    public int WoundsPerModel { get; set; }

    /// <summary>
    /// Number of models in the unit.
    /// Range: 1-50 (individual models to large squads).
    /// </summary>
    public int ModelCount { get; set; }

    /// <summary>
    /// Unit keywords (Infantry, Vehicle, Monster, etc.).
    /// Used for special rules like Anti-Infantry.
    /// </summary>
    public UnitKeyword Keywords { get; set; }

    /// <summary>
    /// Whether the unit is in cover (+1 to save rolls).
    /// </summary>
    public bool InCover { get; set; }

    /// <summary>
    /// Optional description or notes about the unit.
    /// Maximum length: 500 characters (REQ-INPUT-003).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp when this unit profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this unit profile was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the unit profile for correctness.
    /// Implements REQ-INPUT-002 validation.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            errorMessage = "Unit name cannot be empty.";
            return false;
        }

        if (Name.Length > 100)
        {
            errorMessage = "Unit name too long (max 100 characters).";
            return false;
        }

        if (Toughness < 1 || Toughness > 12)
        {
            errorMessage = "Toughness must be between 1 and 12.";
            return false;
        }

        if (Save < 2 || Save > 7)
        {
            errorMessage = "Save must be between 2+ and 7+ (7+ = no save).";
            return false;
        }

        if (InvulnerableSave != 0 && (InvulnerableSave < 2 || InvulnerableSave > 6))
        {
            errorMessage = "Invulnerable save must be 0 (none) or between 2+ and 6+.";
            return false;
        }

        if (FeelNoPain != 0 && (FeelNoPain < 2 || FeelNoPain > 6))
        {
            errorMessage = "Feel No Pain must be 0 (none) or between 2+ and 6+.";
            return false;
        }

        if (WoundsPerModel < 1 || WoundsPerModel > 24)
        {
            errorMessage = "Wounds per model must be between 1 and 24.";
            return false;
        }

        if (ModelCount < 1 || ModelCount > 50)
        {
            errorMessage = "Model count must be between 1 and 50.";
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
    /// Calculates total wounds for the unit.
    /// Implements checked arithmetic (REQ-INPUT-002).
    /// </summary>
    public int TotalWounds()
    {
        try
        {
            return checked(WoundsPerModel * ModelCount);
        }
        catch (OverflowException)
        {
            throw new InvalidOperationException(
                "Total wounds calculation resulted in overflow.");
        }
    }

    /// <summary>
    /// Creates a copy of this unit profile.
    /// </summary>
    public UnitProfile Clone()
    {
        return new UnitProfile
        {
            Id = 0, // New copy gets new ID
            Name = $"{Name} (Copy)",
            Toughness = Toughness,
            Save = Save,
            InvulnerableSave = InvulnerableSave,
            FeelNoPain = FeelNoPain,
            WoundsPerModel = WoundsPerModel,
            ModelCount = ModelCount,
            Keywords = Keywords,
            InCover = InCover,
            Description = Description,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}
