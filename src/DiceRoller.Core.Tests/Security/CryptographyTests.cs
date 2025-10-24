using DiceRoller.Core.Services.Security;
using FluentAssertions;
using Xunit;

namespace DiceRoller.Core.Tests.Security;

/// <summary>
/// Security tests for cryptographic operations (REQ-CRYPTO-001, REQ-CRYPTO-002, REQ-CRYPTO-003).
/// Tests key generation, DPAPI protection, and secure random number generation.
/// </summary>
public class CryptographyTests : IDisposable
{
    private readonly string _testKeyPath;
    private readonly KeyManagementService _keyService;

    public CryptographyTests()
    {
        _testKeyPath = Path.Combine(Path.GetTempPath(), $"test_keys_{Guid.NewGuid()}");
        _keyService = new KeyManagementService(_testKeyPath);
    }

    [Fact]
    public void GenerateKey_ShouldReturn256BitKey()
    {
        // Arrange & Act - REQ-CRYPTO-001: Generate 256-bit key
        byte[] key = _keyService.GenerateKey();

        // Assert
        key.Should().NotBeNull();
        key.Length.Should().Be(32); // 32 bytes = 256 bits
    }

    [Fact]
    public void GenerateKey_MultipleCalls_ShouldReturnDifferentKeys()
    {
        // Arrange & Act - REQ-CRYPTO-003: Cryptographically secure randomness
        byte[] key1 = _keyService.GenerateKey();
        byte[] key2 = _keyService.GenerateKey();
        byte[] key3 = _keyService.GenerateKey();

        // Assert - Keys should be unique (extremely high probability)
        key1.Should().NotBeEquivalentTo(key2);
        key2.Should().NotBeEquivalentTo(key3);
        key1.Should().NotBeEquivalentTo(key3);
    }

    [Fact]
    public void ProtectKey_ShouldReturnDifferentData()
    {
        // Arrange
        byte[] originalKey = _keyService.GenerateKey();

        // Act - REQ-DATA-002: DPAPI protection
        byte[] protectedKey = _keyService.ProtectKey(originalKey);

        // Assert
        protectedKey.Should().NotBeNull();
        protectedKey.Length.Should().BeGreaterThan(0);
        protectedKey.Should().NotBeEquivalentTo(originalKey); // Protected data should differ
    }

    [Fact]
    public void ProtectAndUnprotectKey_ShouldReturnOriginalKey()
    {
        // Arrange
        byte[] originalKey = _keyService.GenerateKey();

        // Act - REQ-DATA-002: DPAPI protect and unprotect
        byte[] protectedKey = _keyService.ProtectKey(originalKey);
        byte[] unprotectedKey = _keyService.UnprotectKey(protectedKey);

        // Assert - Roundtrip should return original key
        unprotectedKey.Should().NotBeNull();
        unprotectedKey.Should().BeEquivalentTo(originalKey);
    }

    [Fact]
    public void ProtectKey_WithNullKey_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _keyService.ProtectKey(null!));
    }

    [Fact]
    public void ProtectKey_WithEmptyKey_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _keyService.ProtectKey(Array.Empty<byte>()));
    }

    [Fact]
    public void ProtectKey_WithInvalidLength_ShouldThrowException()
    {
        // Arrange - Key must be 32 bytes for AES-256
        byte[] invalidKey = new byte[16]; // Only 16 bytes (128 bits)

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _keyService.ProtectKey(invalidKey));
    }

    [Fact]
    public void StoreAndRetrieveProtectedKey_ShouldSucceed()
    {
        // Arrange
        byte[] originalKey = _keyService.GenerateKey();
        byte[] protectedKey = _keyService.ProtectKey(originalKey);
        string keyName = "test_key_1";

        // Act
        _keyService.StoreProtectedKey(protectedKey, keyName);
        byte[]? retrievedKey = _keyService.RetrieveProtectedKey(keyName);

        // Assert
        retrievedKey.Should().NotBeNull();
        retrievedKey.Should().BeEquivalentTo(protectedKey);
    }

    [Fact]
    public void KeyExists_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        byte[] key = _keyService.GenerateKey();
        byte[] protectedKey = _keyService.ProtectKey(key);
        string keyName = "existing_key";

        _keyService.StoreProtectedKey(protectedKey, keyName);

        // Act
        bool exists = _keyService.KeyExists(keyName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void KeyExists_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        string keyName = "nonexistent_key";

        // Act
        bool exists = _keyService.KeyExists(keyName);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void DeleteKey_ShouldRemoveKey()
    {
        // Arrange
        byte[] key = _keyService.GenerateKey();
        byte[] protectedKey = _keyService.ProtectKey(key);
        string keyName = "key_to_delete";

        _keyService.StoreProtectedKey(protectedKey, keyName);

        // Act
        _keyService.DeleteKey(keyName);

        // Assert
        _keyService.KeyExists(keyName).Should().BeFalse();
        _keyService.RetrieveProtectedKey(keyName).Should().BeNull();
    }

    [Theory]
    [InlineData("../../../Windows/System32/key")]
    [InlineData("..\\..\\..\\key")]
    [InlineData("/etc/passwd")]
    public void StoreProtectedKey_WithPathTraversalAttempt_ShouldBeSanitized(string maliciousKeyName)
    {
        // Arrange - REQ-FILE-001: Path traversal prevention
        byte[] key = _keyService.GenerateKey();
        byte[] protectedKey = _keyService.ProtectKey(key);

        // Act - Should either sanitize or throw exception
        try
        {
            _keyService.StoreProtectedKey(protectedKey, maliciousKeyName);

            // If it succeeds, verify it's stored in the safe directory
            string[] files = Directory.GetFiles(_testKeyPath, "*.key", SearchOption.AllDirectories);

            // Assert - File should be in test directory, not traversed path
            files.Should().NotBeEmpty();
            files.All(f => f.StartsWith(_testKeyPath)).Should().BeTrue();
        }
        catch (Exception ex)
        {
            // Throwing an exception is also acceptable security behavior
            ex.Should().BeOfType<ArgumentException>()
                .Or.BeOfType<SecurityException>();
        }
    }

    [Fact]
    public void GenerateKey_ShouldHaveHighEntropy()
    {
        // Arrange & Act - Generate multiple keys and check for randomness
        List<byte[]> keys = new();
        for (int i = 0; i < 10; i++)
        {
            keys.Add(_keyService.GenerateKey());
        }

        // Assert - Keys should have high entropy (not all zeros, not repeating patterns)
        foreach (var key in keys)
        {
            // Check not all zeros
            key.Should().NotBeEquivalentTo(new byte[32]);

            // Check not all same byte
            byte firstByte = key[0];
            key.Should().NotOnlyContain(b => b == firstByte);
        }
    }

    [Fact]
    public void StoreProtectedKey_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        byte[] key = _keyService.GenerateKey();
        byte[] protectedKey = _keyService.ProtectKey(key);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _keyService.StoreProtectedKey(protectedKey, ""));
    }

    [Fact]
    public void RetrieveProtectedKey_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _keyService.RetrieveProtectedKey(""));
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testKeyPath))
        {
            try
            {
                Directory.Delete(_testKeyPath, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
