namespace DiceRoller.Core.Enums;

/// <summary>
/// Unit keywords used for special rules and targeting in Warhammer 40K.
/// </summary>
[Flags]
public enum UnitKeyword
{
    /// <summary>
    /// No special keywords.
    /// </summary>
    None = 0,

    /// <summary>
    /// Infantry unit type.
    /// </summary>
    Infantry = 1 << 0,

    /// <summary>
    /// Vehicle unit type.
    /// </summary>
    Vehicle = 1 << 1,

    /// <summary>
    /// Monster unit type.
    /// </summary>
    Monster = 1 << 2,

    /// <summary>
    /// Character model.
    /// </summary>
    Character = 1 << 3,

    /// <summary>
    /// Fly keyword - can move over terrain.
    /// </summary>
    Fly = 1 << 4,

    /// <summary>
    /// Psyker - can use psychic powers.
    /// </summary>
    Psyker = 1 << 5,

    /// <summary>
    /// Titanic - very large model.
    /// </summary>
    Titanic = 1 << 6,

    /// <summary>
    /// Swarm - multiple small creatures.
    /// </summary>
    Swarm = 1 << 7,

    /// <summary>
    /// Mounted - on a mount.
    /// </summary>
    Mounted = 1 << 8,

    /// <summary>
    /// Daemon - warp entity.
    /// </summary>
    Daemon = 1 << 9
}
