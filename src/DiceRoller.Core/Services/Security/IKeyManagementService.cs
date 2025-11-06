namespace DiceRoller.Core.Services.Security;

/// <summary>
/// Interface for encryption key management.
/// Handles secure generation, storage, and retrieval of database encryption keys.
/// Implements REQ-DATA-002: DPAPI key management.
/// </summary>
public interface IKeyManagementService
{
    /// <summary>
    /// Generates a new database encryption key.
    /// Uses cryptographically secure random number generation (REQ-CRYPTO-001).
    /// </summary>
    /// <returns>32-byte (256-bit) encryption key.</returns>
    byte[] GenerateKey();

    /// <summary>
    /// Protects (encrypts) a key using Windows DPAPI.
    /// The protected key can only be decrypted by the same user on the same machine.
    /// </summary>
    /// <param name="key">The key to protect.</param>
    /// <returns>Protected (encrypted) key data.</returns>
    byte[] ProtectKey(byte[] key);

    /// <summary>
    /// Unprotects (decrypts) a key using Windows DPAPI.
    /// </summary>
    /// <param name="protectedKey">The protected key to decrypt.</param>
    /// <returns>The original unprotected key.</returns>
    byte[] UnprotectKey(byte[] protectedKey);

    /// <summary>
    /// Stores a protected key securely in file-based storage.
    /// Keys are stored in %LocalAppData%\DiceRoller\Keys\ directory with restrictive ACLs.
    /// </summary>
    /// <param name="protectedKey">The protected key to store.</param>
    /// <param name="keyName">Name/identifier for the key.</param>
    void StoreProtectedKey(byte[] protectedKey, string keyName);

    /// <summary>
    /// Retrieves a protected key from file-based storage.
    /// Keys are retrieved from %LocalAppData%\DiceRoller\Keys\ directory.
    /// </summary>
    /// <param name="keyName">Name/identifier of the key to retrieve.</param>
    /// <returns>The protected key, or null if not found.</returns>
    byte[]? RetrieveProtectedKey(string keyName);

    /// <summary>
    /// Checks if a protected key exists in storage.
    /// </summary>
    /// <param name="keyName">Name/identifier of the key.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    bool KeyExists(string keyName);

    /// <summary>
    /// Deletes a protected key from storage.
    /// </summary>
    /// <param name="keyName">Name/identifier of the key to delete.</param>
    void DeleteKey(string keyName);
}
