using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using DiceRoller.Core.Services.Database;
using DiceRoller.Core.Services.Security;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DiceRoller.Core.Tests.Integration;

/// <summary>
/// Integration tests for WeaponRepository.
/// Tests CRUD operations with real database.
/// </summary>
public class WeaponRepositoryTests : IDisposable
{
    private readonly DatabaseService _dbService;
    private readonly WeaponRepository _repository;
    private readonly string _testDatabasePath;
    private readonly byte[] _testEncryptionKey;

    public WeaponRepositoryTests()
    {
        // Setup test database
        string tempDir = Path.Combine(Path.GetTempPath(), "DiceRollerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _testDatabasePath = Path.Combine(tempDir, "test.db");

        var keyService = new KeyManagementService();
        _testEncryptionKey = keyService.GenerateKey();

        _dbService = new DatabaseService();
        _dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        _dbService.CreateSchema();

        _repository = new WeaponRepository(_dbService);
    }

    [Fact]
    public async Task InsertAsync_WithValidWeapon_ShouldReturnId()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Test Bolter",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1,
            Abilities = WeaponAbility.None,
            Description = "Standard bolter"
        };

        // Act
        int id = await _repository.InsertAsync(weapon);

        // Assert
        id.Should().BeGreaterThan(0, "should return valid ID");
        weapon.Id.Should().Be(id, "weapon ID should be set");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnWeapon()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Test Weapon",
            Attacks = 3,
            BallisticSkill = 4,
            Strength = 5,
            ArmorPenetration = -1,
            DamageType = DamageType.D6,
            FixedDamage = 0,
            Abilities = WeaponAbility.RapidFire1,
            Description = "Test description"
        };
        int id = await _repository.InsertAsync(weapon);

        // Act
        var retrieved = await _repository.GetByIdAsync(id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(id);
        retrieved.Name.Should().Be("Test Weapon");
        retrieved.Attacks.Should().Be(3);
        retrieved.BallisticSkill.Should().Be(4);
        retrieved.Strength.Should().Be(5);
        retrieved.ArmorPenetration.Should().Be(-1);
        retrieved.DamageType.Should().Be(DamageType.D6);
        retrieved.Abilities.Should().Be(WeaponAbility.RapidFire1);
        retrieved.Description.Should().Be("Test description");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var weapon = await _repository.GetByIdAsync(99999);

        // Assert
        weapon.Should().BeNull("non-existent ID should return null");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleWeapons_ShouldReturnAll()
    {
        // Arrange
        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Weapon1",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        });

        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Weapon2",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -1,
            DamageType = DamageType.D6,
            FixedDamage = 0
        });

        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Weapon3",
            Attacks = 3,
            BallisticSkill = 4,
            Strength = 6,
            ArmorPenetration = -2,
            DamageType = DamageType.TwoD6,
            FixedDamage = 0
        });

        // Act
        var weapons = await _repository.GetAllAsync();

        // Assert
        weapons.Should().HaveCount(3);
        weapons.Should().Contain(w => w.Name == "Weapon1");
        weapons.Should().Contain(w => w.Name == "Weapon2");
        weapons.Should().Contain(w => w.Name == "Weapon3");
    }

    [Fact]
    public async Task SearchByNameAsync_WithPartialMatch_ShouldReturnMatches()
    {
        // Arrange
        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Heavy Bolter",
            Attacks = 3,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -1,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        });

        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Storm Bolter",
            Attacks = 4,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        });

        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Lascannon",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 12,
            ArmorPenetration = -3,
            DamageType = DamageType.D6Plus3,
            FixedDamage = 0
        });

        // Act
        var results = await _repository.SearchByNameAsync("Bolter");

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(w => w.Name == "Heavy Bolter");
        results.Should().Contain(w => w.Name == "Storm Bolter");
        results.Should().NotContain(w => w.Name == "Lascannon");
    }

    [Fact]
    public async Task SearchByNameAsync_CaseInsensitive_ShouldReturnMatches()
    {
        // Arrange
        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Bolter",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        });

        // Act
        var results = await _repository.SearchByNameAsync("BOLTER");

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Be("Bolter");
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedWeapon_ShouldPersistChanges()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Original Name",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1,
            Description = "Original description"
        };
        int id = await _repository.InsertAsync(weapon);

        // Act - Modify weapon
        weapon.Name = "Updated Name";
        weapon.Attacks = 5;
        weapon.Description = "Updated description";
        weapon.Abilities = WeaponAbility.LethalHits;

        bool updateResult = await _repository.UpdateAsync(weapon);

        // Assert
        updateResult.Should().BeTrue("update should succeed");

        var updated = await _repository.GetByIdAsync(id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Attacks.Should().Be(5);
        updated.Description.Should().Be("Updated description");
        updated.Abilities.Should().Be(WeaponAbility.LethalHits);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Id = 99999,
            Name = "Non-existent",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        // Act
        bool result = await _repository.UpdateAsync(weapon);

        // Assert
        result.Should().BeFalse("update of non-existent weapon should fail");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveWeapon()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "To Delete",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };
        int id = await _repository.InsertAsync(weapon);

        // Act
        bool deleteResult = await _repository.DeleteAsync(id);

        // Assert
        deleteResult.Should().BeTrue("delete should succeed");

        var deleted = await _repository.GetByIdAsync(id);
        deleted.Should().BeNull("deleted weapon should not be retrievable");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse("delete of non-existent weapon should return false");
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Weapon1",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        });

        await _repository.InsertAsync(new WeaponProfile
        {
            Name = "Weapon2",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -1,
            DamageType = DamageType.D6,
            FixedDamage = 0
        });

        // Act
        int count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task InsertAsync_WithInvalidWeapon_ShouldThrowException()
    {
        // Arrange
        var invalidWeapon = new WeaponProfile
        {
            Name = "", // Invalid: empty name
            Attacks = -1, // Invalid: negative attacks
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        // Act & Assert
        Func<Task> act = async () => await _repository.InsertAsync(invalidWeapon);
        await act.Should().ThrowAsync<ArgumentException>("invalid weapon should be rejected");
    }

    [Fact]
    public async Task Repository_WithComplexAbilities_ShouldPersistCorrectly()
    {
        // Arrange - Weapon with multiple abilities (flags enum)
        var weapon = new WeaponProfile
        {
            Name = "Complex Weapon",
            Attacks = 6,
            BallisticSkill = 3,
            Strength = 8,
            ArmorPenetration = -2,
            DamageType = DamageType.D6Plus2,
            FixedDamage = 0,
            Abilities = WeaponAbility.LethalHits | WeaponAbility.SustainedHits1 | WeaponAbility.Melta2,
            AntiThreshold = 3,
            Description = "Multi-ability weapon"
        };

        // Act
        int id = await _repository.InsertAsync(weapon);
        var retrieved = await _repository.GetByIdAsync(id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Abilities.Should().HaveFlag(WeaponAbility.LethalHits);
        retrieved.Abilities.Should().HaveFlag(WeaponAbility.SustainedHits1);
        retrieved.Abilities.Should().HaveFlag(WeaponAbility.Melta2);
        retrieved.AntiThreshold.Should().Be(3);
    }

    [Fact]
    public async Task Repository_CreatedAt_ShouldBeSetAutomatically()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Timestamp Test",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        var beforeInsert = DateTime.UtcNow;

        // Act
        await _repository.InsertAsync(weapon);
        var afterInsert = DateTime.UtcNow;

        // Assert
        weapon.CreatedAt.Should().BeOnOrAfter(beforeInsert);
        weapon.CreatedAt.Should().BeOnOrBefore(afterInsert);
    }

    public void Dispose()
    {
        // Clean up
        _dbService.Dispose();

        try
        {
            string? directory = Path.GetDirectoryName(_testDatabasePath);
            if (directory != null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup
        }

        Array.Clear(_testEncryptionKey, 0, _testEncryptionKey.Length);
    }
}
