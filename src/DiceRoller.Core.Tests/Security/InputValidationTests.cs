using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using FluentAssertions;
using Xunit;

namespace DiceRoller.Core.Tests.Security;

/// <summary>
/// Security tests for input validation (REQ-INPUT-001, REQ-INPUT-002, REQ-INPUT-003).
/// Tests that malicious or invalid inputs are rejected.
/// </summary>
public class InputValidationTests
{
    [Fact]
    public void WeaponProfile_WithNegativeAttacks_ShouldFailValidation()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Test Weapon",
            Attacks = -1,  // Invalid
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Attacks");
    }

    [Fact]
    public void WeaponProfile_WithExcessiveAttacks_ShouldFailValidation()
    {
        // Arrange - REQ-INPUT-002: Prevent integer overflow
        var weapon = new WeaponProfile
        {
            Name = "Test Weapon",
            Attacks = 10000,  // Exceeds maximum of 1000
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("1000");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void WeaponProfile_WithEmptyName_ShouldFailValidation(string? name)
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = name!,
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("name");
    }

    [Fact]
    public void WeaponProfile_WithTooLongName_ShouldFailValidation()
    {
        // Arrange - REQ-INPUT-003: String length validation
        var weapon = new WeaponProfile
        {
            Name = new string('A', 101),  // 101 characters, max is 100
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("100 characters");
    }

    [Fact]
    public void WeaponProfile_WithTooLongDescription_ShouldFailValidation()
    {
        // Arrange - REQ-INPUT-003: String length validation
        var weapon = new WeaponProfile
        {
            Name = "Test Weapon",
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2,
            Description = new string('A', 501)  // 501 characters, max is 500
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("500 characters");
    }

    [Theory]
    [InlineData(1)]  // Too low
    [InlineData(7)]  // Too high
    public void WeaponProfile_WithInvalidBallisticSkill_ShouldFailValidation(int bs)
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Test Weapon",
            Attacks = 10,
            BallisticSkill = bs,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Ballistic Skill");
    }

    [Fact]
    public void UnitProfile_WithNegativeWounds_ShouldFailValidation()
    {
        // Arrange
        var unit = new UnitProfile
        {
            Name = "Test Unit",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = -1,  // Invalid
            ModelCount = 5
        };

        // Act
        bool isValid = unit.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Wounds");
    }

    [Fact]
    public void UnitProfile_WithExcessiveModelCount_ShouldFailValidation()
    {
        // Arrange
        var unit = new UnitProfile
        {
            Name = "Test Unit",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = 1,
            ModelCount = 100  // Exceeds maximum of 50
        };

        // Act
        bool isValid = unit.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("50");
    }

    [Fact]
    public void AttackModifiers_WithExcessiveHitModifier_ShouldFailValidation()
    {
        // Arrange - REQ: 10th edition modifier cap at +1/-1
        var modifiers = new AttackModifiers
        {
            HitModifier = 2  // Exceeds cap of +1
        };

        // Act
        bool isValid = modifiers.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("-1 and +1");
    }

    [Fact]
    public void AttackModifiers_WithExcessiveNegativeWoundModifier_ShouldFailValidation()
    {
        // Arrange - REQ: 10th edition modifier cap at +1/-1
        var modifiers = new AttackModifiers
        {
            WoundModifier = -2  // Exceeds cap of -1
        };

        // Act
        bool isValid = modifiers.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("-1 and +1");
    }

    [Fact]
    public void UnitProfile_TotalWoundsCalculation_WithOverflow_ShouldThrowException()
    {
        // Arrange - REQ-INPUT-002: Integer overflow protection
        var unit = new UnitProfile
        {
            Name = "Test Unit",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = int.MaxValue / 2,
            ModelCount = 10  // This multiplication would overflow
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => unit.TotalWounds());
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("'; DROP TABLE Weapons; --")]
    [InlineData("../../../Windows/System32/malicious.dll")]
    public void WeaponProfile_WithMaliciousName_ShouldAcceptButNotExecute(string maliciousInput)
    {
        // Arrange - REQ-INPUT-003: Accept input but prevent execution
        // The validation should accept the string, but repositories must use parameterized queries
        var weapon = new WeaponProfile
        {
            Name = maliciousInput.Length <= 100 ? maliciousInput : maliciousInput.Substring(0, 100),
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 5,
            ArmorPenetration = -2,
            DamageType = DamageType.Fixed,
            FixedDamage = 2
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        // Should pass validation (input is accepted)
        // SQL injection prevention happens at repository level via parameterized queries
        isValid.Should().BeTrue();
    }

    [Fact]
    public void WeaponProfile_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Bolter",
            Attacks = 2,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        // Act
        bool isValid = weapon.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void UnitProfile_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var unit = new UnitProfile
        {
            Name = "Space Marine",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = 2,
            ModelCount = 10
        };

        // Act
        bool isValid = unit.IsValid(out string? errorMessage);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }
}
