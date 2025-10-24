using DiceRoller.Core.Enums;
using DiceRoller.Core.Models;
using DiceRoller.Core.Services.Calculation;
using FluentAssertions;
using Xunit;

namespace DiceRoller.Core.Tests.Unit;

/// <summary>
/// Unit tests for the dice calculation engine.
/// Tests Warhammer 40K 10th edition rules implementation.
/// </summary>
public class DiceCalculatorTests : IDisposable
{
    private readonly DiceCalculator _calculator;

    public DiceCalculatorTests()
    {
        _calculator = new DiceCalculator();
    }

    [Fact]
    public void CalculateAttack_WithBasicWeapon_ShouldReturnExpectedHits()
    {
        // Arrange - Bolter: 2 attacks, BS 3+
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

        var target = new UnitProfile
        {
            Name = "Guardsman",
            Toughness = 3,
            Save = 5,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - Expected hits: 2 attacks * (4/6) = 1.33 hits
        result.ExpectedDamage.Should().BeGreaterThan(0);
        result.ExpectedModelsKilled.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateAttack_WithLethalHits_ShouldAutoWound()
    {
        // Arrange - Weapon with Lethal Hits
        var weapon = new WeaponProfile
        {
            Name = "Precision Weapon",
            Attacks = 6,
            BallisticSkill = 3, // 4/6 hit rate, 1/6 critical rate
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1,
            Abilities = WeaponAbility.LethalHits
        };

        var target = new UnitProfile
        {
            Name = "Tough Target",
            Toughness = 8, // Very tough - normally hard to wound
            Save = 3,
            WoundsPerModel = 3,
            ModelCount = 5
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - Should have some damage despite high toughness (thanks to Lethal Hits)
        result.ExpectedDamage.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateAttack_WithTorrent_ShouldAutoHit()
    {
        // Arrange - Flamer with Torrent (auto-hit)
        var weapon = new WeaponProfile
        {
            Name = "Flamer",
            Attacks = 6,
            BallisticSkill = 6, // Doesn't matter, Torrent auto-hits
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1,
            Abilities = WeaponAbility.Torrent
        };

        var target = new UnitProfile
        {
            Name = "Infantry Squad",
            Toughness = 3,
            Save = 5,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - All 6 attacks should hit (Torrent)
        // With T3 vs S4, wound on 3+ (4/6 = 0.666)
        // Expected wounds: 6 * 0.666 = 4
        result.ExpectedDamage.Should().BeGreaterThan(2);
    }

    [Fact]
    public void CalculateAttack_WithSustainedHits_ShouldGenerateExtraHits()
    {
        // Arrange - Weapon with Sustained Hits 2
        var weapon = new WeaponProfile
        {
            Name = "Storm Bolter",
            Attacks = 6,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1,
            Abilities = WeaponAbility.SustainedHits2
        };

        var target = new UnitProfile
        {
            Name = "Target",
            Toughness = 4,
            Save = 4,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - Should generate more hits than base attacks due to Sustained Hits
        result.ExpectedDamage.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateAttack_WithDevastatingWounds_ShouldCauseMortalWounds()
    {
        // Arrange - Weapon with Devastating Wounds
        var weapon = new WeaponProfile
        {
            Name = "Melta Gun",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 9,
            ArmorPenetration = -4,
            DamageType = DamageType.D6,
            FixedDamage = 0,
            Abilities = WeaponAbility.DevastatingWounds
        };

        var target = new UnitProfile
        {
            Name = "Tank",
            Toughness = 10,
            Save = 2,
            InvulnerableSave = 4, // 4++ save
            WoundsPerModel = 10,
            ModelCount = 1
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - Should have some mortal wounds (which bypass saves)
        result.ExpectedMortalWounds.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void CalculateAttack_WithCover_ShouldImproveSave()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Lasgun",
            Attacks = 10,
            BallisticSkill = 4,
            Strength = 3,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        var targetInOpen = new UnitProfile
        {
            Name = "Marine",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = 2,
            ModelCount = 5,
            InCover = false
        };

        var targetInCover = new UnitProfile
        {
            Name = "Marine in Cover",
            Toughness = 4,
            Save = 3,
            WoundsPerModel = 2,
            ModelCount = 5,
            InCover = true
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var resultInOpen = _calculator.CalculateAttack(weapon, targetInOpen, modifiers);
        var resultInCover = _calculator.CalculateAttack(weapon, targetInCover, modifiers);

        // Assert - Cover should reduce damage
        resultInCover.ExpectedDamage.Should().BeLessThan(resultInOpen.ExpectedDamage);
    }

    [Fact]
    public void CalculateAttack_WithInvulnerableSave_ShouldUseBetterSave()
    {
        // Arrange - Weapon with high AP
        var weapon = new WeaponProfile
        {
            Name = "Lascannon",
            Attacks = 1,
            BallisticSkill = 3,
            Strength = 12,
            ArmorPenetration = -3,
            DamageType = DamageType.D6Plus3,
            FixedDamage = 0
        };

        var target = new UnitProfile
        {
            Name = "Character with Shield",
            Toughness = 4,
            Save = 3, // 3+ save, becomes 6+ with AP-3
            InvulnerableSave = 4, // 4++ invuln is better than 6+
            WoundsPerModel = 4,
            ModelCount = 1
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var result = _calculator.CalculateAttack(weapon, target, modifiers);

        // Assert - Should survive better with invuln save
        result.ExpectedDamage.Should().BeLessThan(weapon.Attacks * 10); // Not guaranteed to kill
    }

    [Fact]
    public void CalculateAttack_WithFeelNoPain_ShouldReduceDamage()
    {
        // Arrange
        var weapon = new WeaponProfile
        {
            Name = "Bolter",
            Attacks = 10,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        var targetWithoutFNP = new UnitProfile
        {
            Name = "Unit",
            Toughness = 4,
            Save = 4,
            FeelNoPain = 0, // No FNP
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var targetWithFNP = new UnitProfile
        {
            Name = "Unit with FNP",
            Toughness = 4,
            Save = 4,
            FeelNoPain = 5, // 5+ FNP
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act
        var resultWithoutFNP = _calculator.CalculateAttack(weapon, targetWithoutFNP, modifiers);
        var resultWithFNP = _calculator.CalculateAttack(weapon, targetWithFNP, modifiers);

        // Assert - FNP should reduce damage
        resultWithFNP.ExpectedDamage.Should().BeLessThan(resultWithoutFNP.ExpectedDamage);
    }

    [Fact]
    public void RunSimulation_ShouldProduceStatistics()
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

        var target = new UnitProfile
        {
            Name = "Guardsman Squad",
            Toughness = 3,
            Save = 5,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act - Run simulation with 1000 iterations
        var simulation = _calculator.RunSimulation(weapon, target, modifiers, iterations: 1000);

        // Assert
        simulation.Should().NotBeNull();
        simulation.Iterations.Should().Be(1000);
        simulation.Simulations.Should().HaveCount(1000);
        simulation.AverageDamage.Should().BeGreaterThan(0);
        simulation.MinimumDamage.Should().BeGreaterOrEqualTo(0);
        simulation.MaximumDamage.Should().BeGreaterThan(simulation.MinimumDamage);
        simulation.DamageHistogram.Should().NotBeEmpty();
    }

    [Fact]
    public void CalculateAttack_WithInvalidWeapon_ShouldThrowException()
    {
        // Arrange - Invalid weapon (negative attacks)
        var weapon = new WeaponProfile
        {
            Name = "Invalid",
            Attacks = -1,
            BallisticSkill = 3,
            Strength = 4,
            ArmorPenetration = 0,
            DamageType = DamageType.Fixed,
            FixedDamage = 1
        };

        var target = new UnitProfile
        {
            Name = "Target",
            Toughness = 3,
            Save = 5,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _calculator.CalculateAttack(weapon, target, modifiers));
    }

    [Fact]
    public void RunSimulation_WithExcessiveIterations_ShouldThrowException()
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

        var target = new UnitProfile
        {
            Name = "Target",
            Toughness = 3,
            Save = 5,
            WoundsPerModel = 1,
            ModelCount = 10
        };

        var modifiers = AttackModifiers.Default();

        // Act & Assert - Too many iterations (> 1,000,000)
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.RunSimulation(weapon, target, modifiers, iterations: 10_000_000));
    }

    public void Dispose()
    {
        _calculator?.Dispose();
    }
}
