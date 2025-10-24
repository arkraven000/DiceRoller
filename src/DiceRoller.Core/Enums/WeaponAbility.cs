namespace DiceRoller.Core.Enums;

/// <summary>
/// Weapon abilities available in Warhammer 40K 10th Edition.
/// </summary>
[Flags]
public enum WeaponAbility
{
    /// <summary>
    /// No special abilities.
    /// </summary>
    None = 0,

    /// <summary>
    /// Critical hits (unmodified 6 to hit) automatically wound.
    /// Does not require a wound roll.
    /// </summary>
    LethalHits = 1 << 0,

    /// <summary>
    /// Critical wounds (unmodified 6 to wound) inflict mortal wounds equal to damage characteristic.
    /// These mortal wounds bypass normal saves.
    /// </summary>
    DevastatingWounds = 1 << 1,

    /// <summary>
    /// Sustained Hits 1: Critical hits generate 1 additional hit.
    /// </summary>
    SustainedHits1 = 1 << 2,

    /// <summary>
    /// Sustained Hits 2: Critical hits generate 2 additional hits.
    /// </summary>
    SustainedHits2 = 1 << 3,

    /// <summary>
    /// Sustained Hits 3: Critical hits generate 3 additional hits.
    /// </summary>
    SustainedHits3 = 1 << 4,

    /// <summary>
    /// Weapon automatically hits. No hit roll required.
    /// </summary>
    Torrent = 1 << 5,

    /// <summary>
    /// Can re-roll wound rolls.
    /// </summary>
    TwinLinked = 1 << 6,

    /// <summary>
    /// Melta 2: Increased damage at half range (+2 to damage rolls).
    /// </summary>
    Melta2 = 1 << 7,

    /// <summary>
    /// Melta 4: Increased damage at half range (+4 to damage rolls).
    /// </summary>
    Melta4 = 1 << 8,

    /// <summary>
    /// Anti-Infantry X: Wound rolls of X+ against INFANTRY are critical wounds.
    /// </summary>
    AntiInfantry = 1 << 9,

    /// <summary>
    /// Anti-Vehicle X: Wound rolls of X+ against VEHICLE are critical wounds.
    /// </summary>
    AntiVehicle = 1 << 10,

    /// <summary>
    /// Anti-Monster X: Wound rolls of X+ against MONSTER are critical wounds.
    /// </summary>
    AntiMonster = 1 << 11,

    /// <summary>
    /// Rapid Fire 1: Attacking unit within half range makes 1 extra attack.
    /// </summary>
    RapidFire1 = 1 << 12,

    /// <summary>
    /// Rapid Fire 2: Attacking unit within half range makes 2 extra attacks.
    /// </summary>
    RapidFire2 = 1 << 13,

    /// <summary>
    /// Blast: Add 1 attack for every 5 models in target unit.
    /// </summary>
    Blast = 1 << 14,

    /// <summary>
    /// Ignores Cover: Target cannot benefit from cover.
    /// </summary>
    IgnoresCover = 1 << 15,

    /// <summary>
    /// Precision: Can allocate attacks to character models.
    /// </summary>
    Precision = 1 << 16,

    /// <summary>
    /// Hazardous: Roll for hazard after shooting.
    /// </summary>
    Hazardous = 1 << 17
}
