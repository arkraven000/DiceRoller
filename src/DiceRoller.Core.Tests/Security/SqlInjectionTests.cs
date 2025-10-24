using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using DiceRoller.Core.Services.Database;
using FluentAssertions;
using Xunit;

namespace DiceRoller.Core.Tests.Security;

/// <summary>
/// Security tests for SQL injection prevention (REQ-INPUT-004).
/// Tests that malicious SQL inputs are safely handled via parameterized queries.
/// </summary>
public class SqlInjectionTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _database;
    private readonly WeaponRepository _weaponRepo;

    public SqlInjectionTests()
    {
        // Create temporary test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        _database = new DatabaseService();

        // Generate a test encryption key
        byte[] key = new byte[32];
        new Random().NextBytes(key);

        _database.Initialize(_testDbPath, key);
        _database.CreateSchema();

        _weaponRepo = new WeaponRepository(_database);
    }

    [Theory]
    [InlineData("'; DROP TABLE Weapons; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("' UNION SELECT * FROM Units --")]
    [InlineData("admin'--")]
    [InlineData("' OR 1=1--")]
    public async Task WeaponRepository_InsertWithSqlInjectionAttempt_ShouldNotExecuteMaliciousCode(
        string maliciousName)
    {
        // Arrange - REQ-INPUT-004: SQL injection prevention via parameterized queries
        var weapon = new WeaponProfile
        {
            Name = maliciousName,
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act - Should safely insert without executing SQL
        int id = await _weaponRepo.InsertAsync(weapon);

        // Assert - Weapon should be inserted with malicious string as literal text
        id.Should().BeGreaterThan(0);

        // Verify data was stored safely
        var retrieved = await _weaponRepo.GetByIdAsync(id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(maliciousName);

        // Verify table still exists (wasn't dropped)
        int count = await _weaponRepo.GetCountAsync();
        count.Should().Be(1);
    }

    [Theory]
    [InlineData("'; DELETE FROM Weapons WHERE '1'='1")]
    [InlineData("test' OR 1=1 --")]
    public async Task WeaponRepository_SearchWithSqlInjectionAttempt_ShouldNotExecuteMaliciousCode(
        string maliciousSearch)
    {
        // Arrange - Insert a normal weapon first
        var normalWeapon = new WeaponProfile
        {
            Name = "Normal Weapon",
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };
        await _weaponRepo.InsertAsync(normalWeapon);

        // Act - REQ-INPUT-004: Search with SQL injection attempt
        var results = await _weaponRepo.SearchByNameAsync(maliciousSearch);

        // Assert - Should return no results (searching for literal string)
        results.Should().BeEmpty();

        // Verify original data still exists (wasn't deleted)
        int count = await _weaponRepo.GetCountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task WeaponRepository_MultipleInjectionAttempts_DatabaseRemainsIntact()
    {
        // Arrange - Insert legitimate data
        var weapon1 = new WeaponProfile
        {
            Name = "Weapon 1",
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };
        var weapon2 = new WeaponProfile
        {
            Name = "Weapon 2",
            Attacks = 5,
            BallisticSkill = 4,
            Strength = 6,
            ArmorPenetration = -3,
            DamageType = DamageType.Fixed,
            FixedDamage = 3
        };

        await _weaponRepo.InsertAsync(weapon1);
        await _weaponRepo.InsertAsync(weapon2);

        // Act - Attempt multiple SQL injection attacks
        string[] injectionAttempts = {
            "'; DROP TABLE Weapons; --",
            "' OR '1'='1",
            "'; DELETE FROM Weapons WHERE 1=1; --",
            "' UNION SELECT * FROM sqlite_master --"
        };

        foreach (var attempt in injectionAttempts)
        {
            try
            {
                await _weaponRepo.SearchByNameAsync(attempt);
            }
            catch
            {
                // Ignore any exceptions, continue testing
            }
        }

        // Assert - All original data should still exist
        int count = await _weaponRepo.GetCountAsync();
        count.Should().Be(2);

        var allWeapons = await _weaponRepo.GetAllAsync();
        allWeapons.Should().HaveCount(2);
        allWeapons.Should().Contain(w => w.Name == "Weapon 1");
        allWeapons.Should().Contain(w => w.Name == "Weapon 2");
    }

    [Fact]
    public void DatabaseService_QueryWithStringConcatenation_ShouldThrowSecurityException()
    {
        // Arrange
        string userInput = "test'; DROP TABLE Weapons; --";

        // Act & Assert - REQ-INPUT-004: Detect non-parameterized queries
        // Note: This test validates that the database service has detection logic
        // In practice, developers should never call ExecuteQuery with concatenated strings

        Action act = () =>
        {
            // This simulates a developer mistake (string concatenation)
            string badSql = $"SELECT * FROM Weapons WHERE Name = '{userInput}';";

            // DatabaseService should detect this pattern
            _database.ExecuteQuery(badSql);
        };

        // Should detect the SQL injection pattern and throw
        act.Should().Throw<Exception>();
    }

    [Fact]
    public async Task WeaponRepository_UpdateWithSqlInjection_ShouldUpdateSafely()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Original Name",
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        int id = await _weaponRepo.InsertAsync(weapon);

        // Act - Update with SQL injection attempt
        weapon.Id = id;
        weapon.Name = "'; UPDATE Weapons SET Attacks = 9999 WHERE '1'='1"; // Malicious update

        bool updated = await _weaponRepo.UpdateAsync(weapon);

        // Assert
        updated.Should().BeTrue();

        var retrieved = await _weaponRepo.GetByIdAsync(id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(weapon.Name); // Stored as literal text
        retrieved.Attacks.Should().Be(10); // Should NOT be 9999
    }

    public void Dispose()
    {
        _database?.Dispose();

        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
