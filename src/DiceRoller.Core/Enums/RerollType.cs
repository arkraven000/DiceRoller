namespace DiceRoller.Core.Enums;

/// <summary>
/// Types of re-roll modifiers available in Warhammer 40K.
/// </summary>
public enum RerollType
{
    /// <summary>
    /// No re-rolls allowed.
    /// </summary>
    None,

    /// <summary>
    /// Re-roll results of 1.
    /// </summary>
    RerollOnes,

    /// <summary>
    /// Re-roll all failed rolls.
    /// </summary>
    RerollFailed,

    /// <summary>
    /// Re-roll all results (hit, wound, or damage).
    /// </summary>
    RerollAll
}
