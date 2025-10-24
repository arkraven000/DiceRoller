using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using Microsoft.Data.Sqlite;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Implements weapon profile data access with parameterized queries.
/// REQ-INPUT-004: All queries use parameters to prevent SQL injection.
/// REQ-INPUT-001: All inputs validated before database operations.
/// </summary>
public class WeaponRepository : IWeaponRepository
{
    private readonly IDatabaseService _database;

    public WeaponRepository(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Gets all weapon profiles.
    /// REQ-INPUT-004: Uses parameterized query.
    /// </summary>
    public async Task<List<WeaponProfile>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            var weapons = new List<WeaponProfile>();

            using var reader = _database.ExecuteQuery("SELECT * FROM Weapons ORDER BY Name;");

            while (reader.Read())
            {
                weapons.Add(MapFromReader(reader));
            }

            return weapons;
        });
    }

    /// <summary>
    /// Gets a weapon by ID.
    /// REQ-INPUT-004: Uses parameterized query.
    /// </summary>
    public async Task<WeaponProfile?> GetByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var reader = _database.ExecuteQuery(
                "SELECT * FROM Weapons WHERE Id = @id;",
                new SqliteParameter("@id", id));

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        });
    }

    /// <summary>
    /// Searches weapons by name.
    /// REQ-INPUT-004: Uses parameterized query.
    /// REQ-INPUT-003: Search term sanitized via parameter.
    /// </summary>
    public async Task<List<WeaponProfile>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<WeaponProfile>();
        }

        return await Task.Run(() =>
        {
            var weapons = new List<WeaponProfile>();

            // REQ-INPUT-004: Parameterized query prevents SQL injection
            using var reader = _database.ExecuteQuery(
                "SELECT * FROM Weapons WHERE Name LIKE @search ORDER BY Name;",
                new SqliteParameter("@search", $"%{searchTerm}%"));

            while (reader.Read())
            {
                weapons.Add(MapFromReader(reader));
            }

            return weapons;
        });
    }

    /// <summary>
    /// Inserts a new weapon.
    /// REQ-INPUT-001: Validates before insertion.
    /// REQ-INPUT-004: Uses parameterized query.
    /// </summary>
    public async Task<int> InsertAsync(WeaponProfile weapon)
    {
        if (weapon == null)
        {
            throw new ArgumentNullException(nameof(weapon));
        }

        // REQ-INPUT-001: Validate weapon profile
        if (!weapon.IsValid(out string? errorMessage))
        {
            throw new ArgumentException($"Invalid weapon profile: {errorMessage}", nameof(weapon));
        }

        return await Task.Run(() =>
        {
            weapon.CreatedAt = DateTime.UtcNow;
            weapon.ModifiedAt = DateTime.UtcNow;

            // REQ-INPUT-004: Parameterized INSERT
            _database.ExecuteNonQuery(@"
                INSERT INTO Weapons (
                    Name, Attacks, BallisticSkill, Strength, ArmorPenetration,
                    DamageType, FixedDamage, Abilities, AntiThreshold,
                    Description, CreatedAt, ModifiedAt
                ) VALUES (
                    @name, @attacks, @bs, @strength, @ap,
                    @damageType, @fixedDamage, @abilities, @antiThreshold,
                    @description, @createdAt, @modifiedAt
                );",
                new SqliteParameter("@name", weapon.Name),
                new SqliteParameter("@attacks", weapon.Attacks),
                new SqliteParameter("@bs", weapon.BallisticSkill),
                new SqliteParameter("@strength", weapon.Strength),
                new SqliteParameter("@ap", weapon.ArmorPenetration),
                new SqliteParameter("@damageType", (int)weapon.DamageType),
                new SqliteParameter("@fixedDamage", weapon.FixedDamage),
                new SqliteParameter("@abilities", (int)weapon.Abilities),
                new SqliteParameter("@antiThreshold", weapon.AntiThreshold),
                new SqliteParameter("@description", weapon.Description ?? (object)DBNull.Value),
                new SqliteParameter("@createdAt", weapon.CreatedAt.ToString("o")),
                new SqliteParameter("@modifiedAt", weapon.ModifiedAt.ToString("o"))
            );

            // Get the inserted ID
            var id = _database.ExecuteScalar("SELECT last_insert_rowid();");
            return Convert.ToInt32(id);
        });
    }

    /// <summary>
    /// Updates an existing weapon.
    /// REQ-INPUT-001: Validates before update.
    /// REQ-INPUT-004: Uses parameterized query.
    /// </summary>
    public async Task<bool> UpdateAsync(WeaponProfile weapon)
    {
        if (weapon == null)
        {
            throw new ArgumentNullException(nameof(weapon));
        }

        if (weapon.Id <= 0)
        {
            throw new ArgumentException("Weapon ID must be greater than 0.", nameof(weapon));
        }

        // REQ-INPUT-001: Validate weapon profile
        if (!weapon.IsValid(out string? errorMessage))
        {
            throw new ArgumentException($"Invalid weapon profile: {errorMessage}", nameof(weapon));
        }

        return await Task.Run(() =>
        {
            weapon.ModifiedAt = DateTime.UtcNow;

            // REQ-INPUT-004: Parameterized UPDATE
            int rowsAffected = _database.ExecuteNonQuery(@"
                UPDATE Weapons SET
                    Name = @name,
                    Attacks = @attacks,
                    BallisticSkill = @bs,
                    Strength = @strength,
                    ArmorPenetration = @ap,
                    DamageType = @damageType,
                    FixedDamage = @fixedDamage,
                    Abilities = @abilities,
                    AntiThreshold = @antiThreshold,
                    Description = @description,
                    ModifiedAt = @modifiedAt
                WHERE Id = @id;",
                new SqliteParameter("@id", weapon.Id),
                new SqliteParameter("@name", weapon.Name),
                new SqliteParameter("@attacks", weapon.Attacks),
                new SqliteParameter("@bs", weapon.BallisticSkill),
                new SqliteParameter("@strength", weapon.Strength),
                new SqliteParameter("@ap", weapon.ArmorPenetration),
                new SqliteParameter("@damageType", (int)weapon.DamageType),
                new SqliteParameter("@fixedDamage", weapon.FixedDamage),
                new SqliteParameter("@abilities", (int)weapon.Abilities),
                new SqliteParameter("@antiThreshold", weapon.AntiThreshold),
                new SqliteParameter("@description", weapon.Description ?? (object)DBNull.Value),
                new SqliteParameter("@modifiedAt", weapon.ModifiedAt.ToString("o"))
            );

            return rowsAffected > 0;
        });
    }

    /// <summary>
    /// Deletes a weapon by ID.
    /// REQ-INPUT-004: Uses parameterized query.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than 0.", nameof(id));
        }

        return await Task.Run(() =>
        {
            // REQ-INPUT-004: Parameterized DELETE
            int rowsAffected = _database.ExecuteNonQuery(
                "DELETE FROM Weapons WHERE Id = @id;",
                new SqliteParameter("@id", id));

            return rowsAffected > 0;
        });
    }

    /// <summary>
    /// Gets the count of weapons.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await Task.Run(() =>
        {
            var count = _database.ExecuteScalar("SELECT COUNT(*) FROM Weapons;");
            return Convert.ToInt32(count);
        });
    }

    /// <summary>
    /// Maps database reader to WeaponProfile model.
    /// </summary>
    private WeaponProfile MapFromReader(SqliteDataReader reader)
    {
        return new WeaponProfile
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Attacks = reader.GetInt32(reader.GetOrdinal("Attacks")),
            BallisticSkill = reader.GetInt32(reader.GetOrdinal("BallisticSkill")),
            Strength = reader.GetInt32(reader.GetOrdinal("Strength")),
            ArmorPenetration = reader.GetInt32(reader.GetOrdinal("ArmorPenetration")),
            DamageType = (DamageType)reader.GetInt32(reader.GetOrdinal("DamageType")),
            FixedDamage = reader.GetInt32(reader.GetOrdinal("FixedDamage")),
            Abilities = (WeaponAbility)reader.GetInt32(reader.GetOrdinal("Abilities")),
            AntiThreshold = reader.GetInt32(reader.GetOrdinal("AntiThreshold")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString(reader.GetOrdinal("Description")),
            CreatedAt = DateTime.Parse(
                reader.GetString(reader.GetOrdinal("CreatedAt")),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind),
            ModifiedAt = DateTime.Parse(
                reader.GetString(reader.GetOrdinal("ModifiedAt")),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
