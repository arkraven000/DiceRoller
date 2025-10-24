namespace DiceRoller.Core.Enums;

/// <summary>
/// Types of damage characteristics in Warhammer 40K.
/// </summary>
public enum DamageType
{
    /// <summary>
    /// Fixed damage value (e.g., 1, 2, 3).
    /// </summary>
    Fixed,

    /// <summary>
    /// D3 damage (roll 1d6: 1-2=1, 3-4=2, 5-6=3).
    /// </summary>
    D3,

    /// <summary>
    /// D6 damage (roll 1d6).
    /// </summary>
    D6,

    /// <summary>
    /// 2D6 damage (roll 2d6).
    /// </summary>
    TwoD6,

    /// <summary>
    /// D6+1 damage (roll 1d6 and add 1).
    /// </summary>
    D6Plus1,

    /// <summary>
    /// D6+2 damage (roll 1d6 and add 2).
    /// </summary>
    D6Plus2,

    /// <summary>
    /// 2D6+3 damage (roll 2d6 and add 3).
    /// </summary>
    TwoD6Plus3,

    /// <summary>
    /// D3+3 damage (roll 1d3 and add 3).
    /// </summary>
    D3Plus3
}
