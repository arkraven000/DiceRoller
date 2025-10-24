# Secure Coding Guidelines
## Warhammer 40K Dice Calculator

This document provides secure coding guidelines for developers working on the Dice Calculator project.

---

## Table of Contents

1. [Input Validation](#input-validation)
2. [SQL and Database Security](#sql-and-database-security)
3. [Cryptography](#cryptography)
4. [Error Handling](#error-handling)
5. [File Operations](#file-operations)
6. [Memory Safety](#memory-safety)
7. [Dependency Management](#dependency-management)

---

## Input Validation

### REQ-INPUT-001: Validate All User Inputs

**Rule**: All user inputs MUST be validated at the ViewModel layer before processing.

**Good Example**:
```csharp
public class AttackCalculatorViewModel : ObservableObject
{
    private int _numberOfAttacks;

    public int NumberOfAttacks
    {
        get => _numberOfAttacks;
        set
        {
            // Validate range
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    "Number of attacks cannot be negative.");
            }

            if (value > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    "Number of attacks exceeds maximum (1000).");
            }

            SetProperty(ref _numberOfAttacks, value);
        }
    }
}
```

**Bad Example**:
```csharp
// ❌ NO VALIDATION
public int NumberOfAttacks { get; set; }
```

### REQ-INPUT-002: Prevent Integer Overflow

**Rule**: Use checked arithmetic for calculations that could overflow.

**Good Example**:
```csharp
public int CalculateTotalDamage(int attacks, int damagePerHit)
{
    try
    {
        return checked(attacks * damagePerHit);
    }
    catch (OverflowException ex)
    {
        _logger.LogError(ex, "Integer overflow in damage calculation");
        throw new InvalidOperationException(
            "Calculation resulted in too large a number.", ex);
    }
}
```

**Bad Example**:
```csharp
// ❌ NO OVERFLOW PROTECTION
public int CalculateTotalDamage(int attacks, int damagePerHit)
{
    return attacks * damagePerHit;  // Can overflow silently
}
```

### REQ-INPUT-003: Sanitize String Inputs

**Rule**: Validate string length and content.

**Good Example**:
```csharp
public string UnitName
{
    get => _unitName;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Unit name cannot be empty.");
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("Unit name too long (max 100 characters).");
        }

        // Remove potentially dangerous characters
        string sanitized = Regex.Replace(value, @"[<>""']", "");
        SetProperty(ref _unitName, sanitized);
    }
}
```

---

## SQL and Database Security

### REQ-SQL-001: Always Use Parameterized Queries

**Rule**: NEVER concatenate user input into SQL queries.

**Good Example**:
```csharp
public async Task<UnitProfile> GetUnitByNameAsync(string unitName)
{
    const string sql = "SELECT * FROM Units WHERE Name = @name";

    using var command = _connection.CreateCommand();
    command.CommandText = sql;
    command.Parameters.AddWithValue("@name", unitName);

    using var reader = await command.ExecuteReaderAsync();
    // Process results...
}
```

**Bad Example**:
```csharp
// ❌ SQL INJECTION VULNERABILITY
public async Task<UnitProfile> GetUnitByNameAsync(string unitName)
{
    string sql = $"SELECT * FROM Units WHERE Name = '{unitName}'";
    // If unitName = "'; DROP TABLE Units; --" this destroys the database!
}
```

### REQ-SQL-002: Use SQLCipher for Encryption

**Good Example**:
```csharp
public class DatabaseService
{
    public void InitializeDatabase(string dbPath, string encryptionKey)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = encryptionKey  // SQLCipher encryption
        }.ToString();

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        // Configure SQLCipher security settings
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            PRAGMA cipher_page_size = 4096;
            PRAGMA kdf_iter = 256000;
            PRAGMA cipher_hmac_algorithm = HMAC_SHA512;
            PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;
        ";
        command.ExecuteNonQuery();
    }
}
```

---

## Cryptography

### REQ-CRYPTO-001: Use .NET BCL Crypto APIs Only

**Rule**: Never implement custom cryptography. Use System.Security.Cryptography.

**Good Example**:
```csharp
using System.Security.Cryptography;

public class KeyManagementService
{
    public byte[] GenerateDatabaseKey()
    {
        // Use cryptographically secure RNG
        byte[] key = new byte[32];  // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return key;
    }

    public byte[] ProtectKey(byte[] key)
    {
        // Use Windows DPAPI
        return ProtectedData.Protect(
            key,
            null,  // Optional entropy
            DataProtectionScope.CurrentUser
        );
    }

    public byte[] UnprotectKey(byte[] protectedKey)
    {
        return ProtectedData.Unprotect(
            protectedKey,
            null,
            DataProtectionScope.CurrentUser
        );
    }
}
```

**Bad Example**:
```csharp
// ❌ CUSTOM CRYPTO - NEVER DO THIS
public byte[] MyCustomEncryption(byte[] data, byte[] key)
{
    for (int i = 0; i < data.Length; i++)
    {
        data[i] ^= key[i % key.Length];  // Weak XOR cipher
    }
    return data;
}
```

### REQ-CRYPTO-002: No Hardcoded Keys

**Good Example**:
```csharp
public class DatabaseKeyProvider
{
    private readonly string _keyStorePath;

    public byte[] GetKey()
    {
        // Read encrypted key from secure storage
        byte[] protectedKey = File.ReadAllBytes(_keyStorePath);

        // Decrypt using DPAPI
        return ProtectedData.Unprotect(
            protectedKey,
            null,
            DataProtectionScope.CurrentUser
        );
    }
}
```

**Bad Example**:
```csharp
// ❌ HARDCODED KEY - NEVER DO THIS
private const string DATABASE_KEY = "MySecretKey123!";
```

### REQ-CRYPTO-003: Use Secure RNG for Dice Rolls

**Good Example**:
```csharp
public class DiceRoller
{
    private readonly RandomNumberGenerator _rng;

    public DiceRoller()
    {
        _rng = RandomNumberGenerator.Create();
    }

    public int RollD6()
    {
        byte[] randomBytes = new byte[4];
        _rng.GetBytes(randomBytes);
        int randomValue = BitConverter.ToInt32(randomBytes, 0);

        // Convert to 1-6 range
        return (Math.Abs(randomValue) % 6) + 1;
    }
}
```

**Bad Example**:
```csharp
// ❌ WEAK RNG - PREDICTABLE
private Random _random = new Random();

public int RollD6()
{
    return _random.Next(1, 7);  // Not cryptographically secure
}
```

---

## Error Handling

### REQ-ERROR-001: Implement Graceful Error Handling

**Good Example**:
```csharp
public async Task<Result<UnitProfile>> LoadUnitAsync(int id)
{
    try
    {
        var unit = await _database.GetUnitByIdAsync(id);

        if (unit == null)
        {
            _logger.LogWarning("Unit with ID {UnitId} not found", id);
            return Result<UnitProfile>.Failure("Unit not found");
        }

        return Result<UnitProfile>.Success(unit);
    }
    catch (SqliteException ex)
    {
        _logger.LogError(ex, "Database error loading unit {UnitId}", id);
        return Result<UnitProfile>.Failure("Failed to load unit");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error loading unit {UnitId}", id);
        return Result<UnitProfile>.Failure("An unexpected error occurred");
    }
}
```

### REQ-ERROR-002: Never Expose Sensitive Information

**Good Example**:
```csharp
catch (Exception ex)
{
    // Log detailed error securely
    _logger.LogError(ex, "Failed to open database at {Path}", dbPath);

    // Show generic message to user
    await ShowMessageAsync("Unable to open database. Please check the application logs.");
}
```

**Bad Example**:
```csharp
// ❌ INFORMATION DISCLOSURE
catch (Exception ex)
{
    // Shows full exception to user including file paths, stack traces, etc.
    MessageBox.Show(ex.ToString());
}
```

---

## File Operations

### REQ-FILE-001: Validate File Paths

**Good Example**:
```csharp
public async Task ExportDataAsync(string filePath)
{
    // Validate path
    if (string.IsNullOrWhiteSpace(filePath))
    {
        throw new ArgumentException("File path cannot be empty");
    }

    // Canonicalize path to prevent traversal
    string fullPath = Path.GetFullPath(filePath);

    // Ensure it's in an allowed location
    string appDataPath = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData);

    if (!fullPath.StartsWith(appDataPath, StringComparison.OrdinalIgnoreCase))
    {
        throw new SecurityException("Cannot write outside application data folder");
    }

    // Proceed with file operation
    await File.WriteAllTextAsync(fullPath, data);
}
```

**Bad Example**:
```csharp
// ❌ PATH TRAVERSAL VULNERABILITY
public async Task ExportDataAsync(string filePath)
{
    // No validation - user could provide "../../../Windows/System32/malicious.dll"
    await File.WriteAllTextAsync(filePath, data);
}
```

### REQ-FILE-002: Set Secure File Permissions

**Good Example**:
```csharp
public void CreateDatabaseFile(string path)
{
    // Create file
    File.WriteAllBytes(path, Array.Empty<byte>());

    // Set permissions: Current user only
    var fileInfo = new FileInfo(path);
    var fileSecurity = fileInfo.GetAccessControl();

    // Remove inherited permissions
    fileSecurity.SetAccessRuleProtection(true, false);

    // Add current user with full control
    var currentUser = WindowsIdentity.GetCurrent().User;
    fileSecurity.AddAccessRule(new FileSystemAccessRule(
        currentUser,
        FileSystemRights.FullControl,
        AccessControlType.Allow
    ));

    fileInfo.SetAccessControl(fileSecurity);
}
```

---

## Memory Safety

### REQ-MEM-001: Dispose Resources Properly

**Good Example**:
```csharp
public async Task<List<UnitProfile>> LoadAllUnitsAsync()
{
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Units";

    using var reader = await command.ExecuteReaderAsync();

    var units = new List<UnitProfile>();
    while (await reader.ReadAsync())
    {
        units.Add(MapToUnitProfile(reader));
    }

    return units;
}
```

**Bad Example**:
```csharp
// ❌ RESOURCE LEAK
public List<UnitProfile> LoadAllUnits()
{
    var connection = new SqliteConnection(_connectionString);
    connection.Open();
    // Connection never disposed!

    var command = connection.CreateCommand();
    // Command never disposed!
}
```

### REQ-MEM-002: Clear Sensitive Data from Memory

**Good Example**:
```csharp
public void ProcessEncryptionKey(byte[] key)
{
    try
    {
        // Use the key
        EncryptDatabase(key);
    }
    finally
    {
        // Clear sensitive data from memory
        if (key != null)
        {
            Array.Clear(key, 0, key.Length);
        }
    }
}
```

---

## Dependency Management

### REQ-DEP-001: Pin Package Versions

**Good Example**:
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.10" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlcipher" Version="2.1.10" />
```

**Bad Example**:
```xml
<!-- ❌ FLOATING VERSION -->
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.*" />
```

### REQ-DEP-002: Verify Package Sources

**Checklist**:
- ✅ Package from verified publisher (Microsoft, .NET Foundation, etc.)
- ✅ Package has 1M+ downloads OR extensive community review
- ✅ Package actively maintained (recent updates)
- ✅ No known vulnerabilities in NuGet audit
- ✅ License compatible with project (MIT, Apache 2.0, etc.)

### REQ-DEP-003: Regular Dependency Updates

**Process**:
1. Check for updates weekly
2. Review release notes for security fixes
3. Update dependencies in development branch
4. Run full test suite
5. Deploy to production if tests pass

---

## Code Review Checklist

Before submitting code for review, verify:

- [ ] All user inputs validated
- [ ] No SQL string concatenation
- [ ] No hardcoded secrets or keys
- [ ] Only .NET BCL crypto used
- [ ] Error messages don't expose sensitive data
- [ ] File paths validated and canonicalized
- [ ] Resources properly disposed (using statements)
- [ ] Package versions pinned
- [ ] All security analyzer warnings resolved
- [ ] Unit tests added for new functionality
- [ ] Security tests added for security-critical code

---

## Security Testing

### Unit Test Example: SQL Injection Prevention

```csharp
[Theory]
[InlineData("'; DROP TABLE Units; --")]
[InlineData("' OR '1'='1")]
[InlineData("<script>alert('xss')</script>")]
public async Task GetUnitByName_WithMaliciousInput_DoesNotExecuteMaliciousCode(
    string maliciousInput)
{
    // Arrange
    var service = new UnitService(_mockDatabase.Object);

    // Act
    var result = await service.GetUnitByNameAsync(maliciousInput);

    // Assert
    result.Should().BeNull();  // Should not find unit
    _mockDatabase.Verify(
        x => x.ExecuteNonQueryAsync(It.IsAny<string>()),
        Times.Never);  // Should never execute DROP or other commands
}
```

### Integration Test Example: Database Encryption

```csharp
[Fact]
public void DatabaseFile_IsEncrypted()
{
    // Arrange
    var dbPath = Path.Combine(_testDirectory, "test.db");
    var service = new DatabaseService();
    service.Initialize(dbPath, "test-key");

    // Act
    service.CreateUnit(new UnitProfile { Name = "Test Unit" });
    service.Dispose();

    // Assert - Try to read with standard SQLite (should fail)
    var plainConnection = new SqliteConnection($"Data Source={dbPath}");
    Action openPlainConnection = () => plainConnection.Open();

    openPlainConnection.Should().Throw<SqliteException>()
        .WithMessage("*file is not a database*");
}
```

---

## Resources

- [OWASP Secure Coding Practices](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [.NET Security Documentation](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [SQLCipher Documentation](https://www.zetetic.net/sqlcipher/documentation/)

---

**Last Updated**: 2025-10-24
**Version**: 1.0
