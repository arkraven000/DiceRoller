using DiceRoller.Core.Services.Database;
using DiceRoller.Core.Services.Security;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace DiceRoller.Core.Tests.Integration;

/// <summary>
/// Integration tests for DatabaseService.
/// Tests SQLCipher encryption, schema creation, and query execution.
/// </summary>
public class DatabaseServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly byte[] _testEncryptionKey;
    private readonly KeyManagementService _keyService;

    public DatabaseServiceTests()
    {
        // Create temporary directory for test databases
        string tempDir = Path.Combine(Path.GetTempPath(), "DiceRollerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _testDatabasePath = Path.Combine(tempDir, "test.db");

        // Generate test encryption key
        _keyService = new KeyManagementService();
        _testEncryptionKey = _keyService.GenerateKey();
    }

    [Fact]
    public void Initialize_WithValidPath_ShouldCreateDatabase()
    {
        // Arrange
        using var dbService = new DatabaseService();

        // Act
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Assert
        File.Exists(_testDatabasePath).Should().BeTrue("database file should be created");
        dbService.IsInitialized.Should().BeTrue();
        dbService.Connection.Should().NotBeNull();
    }

    [Fact]
    public void Initialize_WithInvalidPath_ShouldThrowException()
    {
        // Arrange
        using var dbService = new DatabaseService();
        string invalidPath = "Z:\\NonExistentDrive\\invalid.db";

        // Act & Assert
        Action act = () => dbService.Initialize(invalidPath, _testEncryptionKey);
        act.Should().Throw<Exception>("invalid paths should be rejected");
    }

    [Fact]
    public void Initialize_WithNullEncryptionKey_ShouldThrowException()
    {
        // Arrange
        using var dbService = new DatabaseService();

        // Act & Assert
        Action act = () => dbService.Initialize(_testDatabasePath, null!);
        act.Should().Throw<ArgumentNullException>("null encryption key should be rejected");
    }

    [Fact]
    public void Initialize_WithEmptyEncryptionKey_ShouldThrowException()
    {
        // Arrange
        using var dbService = new DatabaseService();
        byte[] emptyKey = Array.Empty<byte>();

        // Act & Assert
        Action act = () => dbService.Initialize(_testDatabasePath, emptyKey);
        act.Should().Throw<ArgumentException>("empty encryption key should be rejected");
    }

    [Fact]
    public void Initialize_CalledTwice_ShouldThrowException()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Act & Assert
        Action act = () => dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        act.Should().Throw<InvalidOperationException>("double initialization should be prevented");
    }

    [Fact]
    public void CreateSchema_ShouldCreateTablesAndConstraints()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Act
        dbService.CreateSchema();

        // Assert - Verify tables exist
        var tables = dbService.ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table'");
        var tableNames = new System.Collections.Generic.List<string>();
        while (tables.Read())
        {
            tableNames.Add(tables.GetString(0));
        }
        tables.Close();

        tableNames.Should().Contain("Weapons", "Weapons table should exist");
        tableNames.Should().Contain("Units", "Units table should exist");
    }

    [Fact]
    public void CreateSchema_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Act - Call twice
        dbService.CreateSchema();
        Action act = () => dbService.CreateSchema();

        // Assert - Should not throw
        act.Should().NotThrow("schema creation should be idempotent");
    }

    [Fact]
    public void ExecuteQuery_WithParameters_ShouldReturnResults()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        dbService.CreateSchema();

        // Insert test data
        var param = new Microsoft.Data.Sqlite.SqliteParameter("@name", "Test Weapon");
        dbService.ExecuteNonQuery("INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES (@name, 1, 3, 4, 0, 1, 0)", param);

        // Act
        var result = dbService.ExecuteQuery("SELECT Name FROM Weapons WHERE Name = @name", param);

        // Assert
        result.Read().Should().BeTrue("query should return result");
        result.GetString(0).Should().Be("Test Weapon");
        result.Close();
    }

    [Fact]
    public void ExecuteNonQuery_WithInsert_ShouldReturnRowsAffected()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        dbService.CreateSchema();

        var param = new Microsoft.Data.Sqlite.SqliteParameter("@name", "Test Weapon");

        // Act
        int rowsAffected = dbService.ExecuteNonQuery(
            "INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES (@name, 1, 3, 4, 0, 1, 0)",
            param);

        // Assert
        rowsAffected.Should().Be(1, "one row should be inserted");
    }

    [Fact]
    public void ExecuteScalar_WithCount_ShouldReturnValue()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        dbService.CreateSchema();

        // Insert test data
        dbService.ExecuteNonQuery("INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES ('Weapon1', 1, 3, 4, 0, 1, 0)");
        dbService.ExecuteNonQuery("INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES ('Weapon2', 2, 3, 5, -1, 2, 0)");

        // Act
        object? result = dbService.ExecuteScalar("SELECT COUNT(*) FROM Weapons");

        // Assert
        result.Should().NotBeNull();
        Convert.ToInt64(result).Should().Be(2, "should count 2 weapons");
    }

    [Fact]
    public void Transaction_Commit_ShouldPersistChanges()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        dbService.CreateSchema();

        // Act
        using (var transaction = dbService.BeginTransaction())
        {
            dbService.ExecuteNonQuery("INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES ('Weapon1', 1, 3, 4, 0, 1, 0)");
            transaction.Commit();
        }

        // Assert
        object? count = dbService.ExecuteScalar("SELECT COUNT(*) FROM Weapons");
        Convert.ToInt64(count).Should().Be(1, "committed transaction should persist");
    }

    [Fact]
    public void Transaction_Rollback_ShouldRevertChanges()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);
        dbService.CreateSchema();

        // Act
        using (var transaction = dbService.BeginTransaction())
        {
            dbService.ExecuteNonQuery("INSERT INTO Weapons (Name, Attacks, BS, Strength, AP, Damage, Abilities) VALUES ('Weapon1', 1, 3, 4, 0, 1, 0)");
            transaction.Rollback();
        }

        // Assert
        object? count = dbService.ExecuteScalar("SELECT COUNT(*) FROM Weapons");
        Convert.ToInt64(count).Should().Be(0, "rolled back transaction should not persist");
    }

    [Fact]
    public void Database_WithWrongKey_ShouldFailToOpen()
    {
        // Arrange - Create database with one key
        using (var dbService1 = new DatabaseService())
        {
            dbService1.Initialize(_testDatabasePath, _testEncryptionKey);
            dbService1.CreateSchema();
            dbService1.Close();
        }

        // Act & Assert - Try to open with different key
        using var dbService2 = new DatabaseService();
        byte[] wrongKey = _keyService.GenerateKey();

        Action act = () =>
        {
            dbService2.Initialize(_testDatabasePath, wrongKey);
            // Try to query - this should fail if encryption is working
            dbService2.ExecuteScalar("SELECT COUNT(*) FROM sqlite_master");
        };

        act.Should().Throw<Microsoft.Data.Sqlite.SqliteException>("wrong encryption key should fail");
    }

    [Fact]
    public void Database_AfterClose_ConnectionShouldBeClosed()
    {
        // Arrange
        using var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Act
        dbService.Close();

        // Assert
        dbService.Connection.State.Should().Be(System.Data.ConnectionState.Closed);
    }

    [Fact]
    public void Dispose_ShouldCloseConnection()
    {
        // Arrange
        var dbService = new DatabaseService();
        dbService.Initialize(_testDatabasePath, _testEncryptionKey);

        // Act
        dbService.Dispose();

        // Assert
        dbService.Connection.State.Should().Be(System.Data.ConnectionState.Closed);
    }

    public void Dispose()
    {
        // Clean up test database and directory
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

        // Clear encryption key from memory
        Array.Clear(_testEncryptionKey, 0, _testEncryptionKey.Length);
    }
}
