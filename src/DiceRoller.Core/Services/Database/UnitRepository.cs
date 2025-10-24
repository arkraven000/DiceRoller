using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using Microsoft.Data.Sqlite;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Implements unit profile data access with parameterized queries.
/// REQ-INPUT-004: All queries use parameters to prevent SQL injection.
/// REQ-INPUT-001: All inputs validated before database operations.
/// </summary>
public class UnitRepository : IUnitRepository
{
    private readonly IDatabaseService _database;

    public UnitRepository(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<List<UnitProfile>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            var units = new List<UnitProfile>();

            using var reader = _database.ExecuteQuery("SELECT * FROM Units ORDER BY Name;");

            while (reader.Read())
            {
                units.Add(MapFromReader(reader));
            }

            return units;
        });
    }

    public async Task<UnitProfile?> GetByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var reader = _database.ExecuteQuery(
                "SELECT * FROM Units WHERE Id = @id;",
                new SqliteParameter("@id", id));

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        });
    }

    public async Task<List<UnitProfile>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<UnitProfile>();
        }

        return await Task.Run(() =>
        {
            var units = new List<UnitProfile>();

            using var reader = _database.ExecuteQuery(
                "SELECT * FROM Units WHERE Name LIKE @search ORDER BY Name;",
                new SqliteParameter("@search", $"%{searchTerm}%"));

            while (reader.Read())
            {
                units.Add(MapFromReader(reader));
            }

            return units;
        });
    }

    public async Task<int> InsertAsync(UnitProfile unit)
    {
        if (unit == null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        // REQ-INPUT-001: Validate unit profile
        if (!unit.IsValid(out string? errorMessage))
        {
            throw new ArgumentException($"Invalid unit profile: {errorMessage}", nameof(unit));
        }

        return await Task.Run(() =>
        {
            unit.CreatedAt = DateTime.UtcNow;
            unit.ModifiedAt = DateTime.UtcNow;

            _database.ExecuteNonQuery(@"
                INSERT INTO Units (
                    Name, Toughness, Save, InvulnerableSave, FeelNoPain,
                    WoundsPerModel, ModelCount, Keywords, InCover,
                    Description, CreatedAt, ModifiedAt
                ) VALUES (
                    @name, @toughness, @save, @invuln, @fnp,
                    @wounds, @modelCount, @keywords, @inCover,
                    @description, @createdAt, @modifiedAt
                );",
                new SqliteParameter("@name", unit.Name),
                new SqliteParameter("@toughness", unit.Toughness),
                new SqliteParameter("@save", unit.Save),
                new SqliteParameter("@invuln", unit.InvulnerableSave),
                new SqliteParameter("@fnp", unit.FeelNoPain),
                new SqliteParameter("@wounds", unit.WoundsPerModel),
                new SqliteParameter("@modelCount", unit.ModelCount),
                new SqliteParameter("@keywords", (int)unit.Keywords),
                new SqliteParameter("@inCover", unit.InCover ? 1 : 0),
                new SqliteParameter("@description", unit.Description ?? (object)DBNull.Value),
                new SqliteParameter("@createdAt", unit.CreatedAt.ToString("o")),
                new SqliteParameter("@modifiedAt", unit.ModifiedAt.ToString("o"))
            );

            var id = _database.ExecuteScalar("SELECT last_insert_rowid();");
            return Convert.ToInt32(id);
        });
    }

    public async Task<bool> UpdateAsync(UnitProfile unit)
    {
        if (unit == null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        if (unit.Id <= 0)
        {
            throw new ArgumentException("Unit ID must be greater than 0.", nameof(unit));
        }

        // REQ-INPUT-001: Validate unit profile
        if (!unit.IsValid(out string? errorMessage))
        {
            throw new ArgumentException($"Invalid unit profile: {errorMessage}", nameof(unit));
        }

        return await Task.Run(() =>
        {
            unit.ModifiedAt = DateTime.UtcNow;

            int rowsAffected = _database.ExecuteNonQuery(@"
                UPDATE Units SET
                    Name = @name,
                    Toughness = @toughness,
                    Save = @save,
                    InvulnerableSave = @invuln,
                    FeelNoPain = @fnp,
                    WoundsPerModel = @wounds,
                    ModelCount = @modelCount,
                    Keywords = @keywords,
                    InCover = @inCover,
                    Description = @description,
                    ModifiedAt = @modifiedAt
                WHERE Id = @id;",
                new SqliteParameter("@id", unit.Id),
                new SqliteParameter("@name", unit.Name),
                new SqliteParameter("@toughness", unit.Toughness),
                new SqliteParameter("@save", unit.Save),
                new SqliteParameter("@invuln", unit.InvulnerableSave),
                new SqliteParameter("@fnp", unit.FeelNoPain),
                new SqliteParameter("@wounds", unit.WoundsPerModel),
                new SqliteParameter("@modelCount", unit.ModelCount),
                new SqliteParameter("@keywords", (int)unit.Keywords),
                new SqliteParameter("@inCover", unit.InCover ? 1 : 0),
                new SqliteParameter("@description", unit.Description ?? (object)DBNull.Value),
                new SqliteParameter("@modifiedAt", unit.ModifiedAt.ToString("o"))
            );

            return rowsAffected > 0;
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than 0.", nameof(id));
        }

        return await Task.Run(() =>
        {
            int rowsAffected = _database.ExecuteNonQuery(
                "DELETE FROM Units WHERE Id = @id;",
                new SqliteParameter("@id", id));

            return rowsAffected > 0;
        });
    }

    public async Task<int> GetCountAsync()
    {
        return await Task.Run(() =>
        {
            var count = _database.ExecuteScalar("SELECT COUNT(*) FROM Units;");
            return Convert.ToInt32(count);
        });
    }

    private UnitProfile MapFromReader(SqliteDataReader reader)
    {
        return new UnitProfile
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Toughness = reader.GetInt32(reader.GetOrdinal("Toughness")),
            Save = reader.GetInt32(reader.GetOrdinal("Save")),
            InvulnerableSave = reader.GetInt32(reader.GetOrdinal("InvulnerableSave")),
            FeelNoPain = reader.GetInt32(reader.GetOrdinal("FeelNoPain")),
            WoundsPerModel = reader.GetInt32(reader.GetOrdinal("WoundsPerModel")),
            ModelCount = reader.GetInt32(reader.GetOrdinal("ModelCount")),
            Keywords = (UnitKeyword)reader.GetInt32(reader.GetOrdinal("Keywords")),
            InCover = reader.GetInt32(reader.GetOrdinal("InCover")) == 1,
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
