using DiceRoller.Core.Models;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Repository interface for unit profile data access.
/// All operations use parameterized queries (REQ-INPUT-004).
/// </summary>
public interface IUnitRepository
{
    /// <summary>
    /// Gets all unit profiles.
    /// </summary>
    Task<List<UnitProfile>> GetAllAsync();

    /// <summary>
    /// Gets a unit profile by ID.
    /// </summary>
    Task<UnitProfile?> GetByIdAsync(int id);

    /// <summary>
    /// Searches for units by name (case-insensitive).
    /// </summary>
    Task<List<UnitProfile>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Inserts a new unit profile.
    /// REQ-INPUT-001: Validates unit before insertion.
    /// </summary>
    Task<int> InsertAsync(UnitProfile unit);

    /// <summary>
    /// Updates an existing unit profile.
    /// REQ-INPUT-001: Validates unit before update.
    /// </summary>
    Task<bool> UpdateAsync(UnitProfile unit);

    /// <summary>
    /// Deletes a unit profile by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Gets the total count of units.
    /// </summary>
    Task<int> GetCountAsync();
}
