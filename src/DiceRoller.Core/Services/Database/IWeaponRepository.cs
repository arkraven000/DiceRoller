using DiceRoller.Core.Models;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Repository interface for weapon profile data access.
/// All operations use parameterized queries (REQ-INPUT-004).
/// </summary>
public interface IWeaponRepository
{
    /// <summary>
    /// Gets all weapon profiles.
    /// </summary>
    Task<List<WeaponProfile>> GetAllAsync();

    /// <summary>
    /// Gets a weapon profile by ID.
    /// </summary>
    Task<WeaponProfile?> GetByIdAsync(int id);

    /// <summary>
    /// Searches for weapons by name (case-insensitive).
    /// </summary>
    Task<List<WeaponProfile>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Inserts a new weapon profile.
    /// REQ-INPUT-001: Validates weapon before insertion.
    /// </summary>
    Task<int> InsertAsync(WeaponProfile weapon);

    /// <summary>
    /// Updates an existing weapon profile.
    /// REQ-INPUT-001: Validates weapon before update.
    /// </summary>
    Task<bool> UpdateAsync(WeaponProfile weapon);

    /// <summary>
    /// Deletes a weapon profile by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Gets the total count of weapons.
    /// </summary>
    Task<int> GetCountAsync();
}
