using Microsoft.Data.Sqlite;
using System.Text;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Implements SQLCipher encrypted database operations.
/// REQ-DATA-001: AES-256 encryption using SQLCipher.
/// REQ-INPUT-004: Parameterized queries only (no string concatenation).
/// REQ-DATA-003: Database stored in user's AppData directory.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private SqliteConnection? _connection;
    private bool _disposed;

    public SqliteConnection Connection
    {
        get
        {
            if (_connection == null || !IsInitialized)
            {
                throw new InvalidOperationException(
                    "Database is not initialized. Call Initialize() first.");
            }
            return _connection;
        }
    }

    public bool IsInitialized => _connection != null && _connection.State == System.Data.ConnectionState.Open;

    /// <summary>
    /// Initializes the database with encryption.
    /// REQ-DATA-001: SQLCipher AES-256 encryption.
    /// REQ-DATA-002: Uses key from DPAPI key management.
    /// </summary>
    public void Initialize(string databasePath, byte[] encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            throw new ArgumentException(
                "Encryption key must be 32 bytes (256 bits) for AES-256.",
                nameof(encryptionKey));
        }

        try
        {
            // REQ-FILE-001: Validate and canonicalize path
            string fullPath = Path.GetFullPath(databasePath);
            string directory = Path.GetDirectoryName(fullPath)!;

            // Ensure directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Build connection string with encryption
            // Note: SQLCipher requires the password to be set as a hex string
            string hexKey = Convert.ToHexString(encryptionKey);

            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = fullPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                Password = hexKey  // SQLCipher encryption key
            };

            _connection = new SqliteConnection(connectionStringBuilder.ToString());
            _connection.Open();

            // Configure SQLCipher security settings
            // REQ-DATA-001: Strong encryption parameters
            ConfigureSQLCipher();

            // Verify encryption is working
            VerifyEncryption();
        }
        catch (SqliteException ex)
        {
            throw new InvalidOperationException(
                "Failed to initialize database. Ensure SQLCipher is properly installed.",
                ex);
        }
        finally
        {
            // REQ-MEM-002: Clear encryption key from memory
            Array.Clear(encryptionKey, 0, encryptionKey.Length);
        }
    }

    /// <summary>
    /// Configures SQLCipher security settings for maximum protection.
    /// REQ-DATA-001: Strong KDF iterations and algorithms.
    /// </summary>
    private void ConfigureSQLCipher()
    {
        if (_connection == null) return;

        try
        {
            // Set SQLCipher parameters for strong security
            using var command = _connection.CreateCommand();

            // Use multiple statements to configure SQLCipher
            command.CommandText = @"
                PRAGMA cipher_page_size = 4096;
                PRAGMA kdf_iter = 256000;
                PRAGMA cipher_hmac_algorithm = HMAC_SHA512;
                PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;
            ";

            command.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            // If SQLCipher pragmas fail, we might not have SQLCipher installed
            throw new InvalidOperationException(
                "Failed to configure SQLCipher. Ensure SQLCipher extension is installed.",
                ex);
        }
    }

    /// <summary>
    /// Verifies that encryption is active by checking cipher_version.
    /// </summary>
    private void VerifyEncryption()
    {
        if (_connection == null) return;

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "PRAGMA cipher_version;";

            var version = command.ExecuteScalar();
            if (version == null)
            {
                throw new SecurityException(
                    "SQLCipher encryption not active. Database may not be encrypted.");
            }
        }
        catch (SqliteException ex)
        {
            throw new SecurityException(
                "Failed to verify database encryption.",
                ex);
        }
    }

    /// <summary>
    /// Creates the database schema for storing weapon and unit profiles.
    /// </summary>
    public void CreateSchema()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Database is not initialized.");
        }

        using var transaction = BeginTransaction();

        try
        {
            // Create Weapons table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Weapons (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Attacks INTEGER NOT NULL,
                    BallisticSkill INTEGER NOT NULL,
                    Strength INTEGER NOT NULL,
                    ArmorPenetration INTEGER NOT NULL,
                    DamageType INTEGER NOT NULL,
                    FixedDamage INTEGER NOT NULL,
                    Abilities INTEGER NOT NULL,
                    AntiThreshold INTEGER NOT NULL,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL,
                    CONSTRAINT chk_name_length CHECK (length(Name) <= 100),
                    CONSTRAINT chk_attacks CHECK (Attacks >= 1 AND Attacks <= 1000),
                    CONSTRAINT chk_bs CHECK (BallisticSkill >= 2 AND BallisticSkill <= 6),
                    CONSTRAINT chk_strength CHECK (Strength >= 1 AND Strength <= 20),
                    CONSTRAINT chk_ap CHECK (ArmorPenetration >= -6 AND ArmorPenetration <= 0)
                );
            ");

            // Create Units table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Units (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Toughness INTEGER NOT NULL,
                    Save INTEGER NOT NULL,
                    InvulnerableSave INTEGER NOT NULL,
                    FeelNoPain INTEGER NOT NULL,
                    WoundsPerModel INTEGER NOT NULL,
                    ModelCount INTEGER NOT NULL,
                    Keywords INTEGER NOT NULL,
                    InCover INTEGER NOT NULL,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL,
                    CONSTRAINT chk_unit_name_length CHECK (length(Name) <= 100),
                    CONSTRAINT chk_toughness CHECK (Toughness >= 1 AND Toughness <= 12),
                    CONSTRAINT chk_save CHECK (Save >= 2 AND Save <= 7),
                    CONSTRAINT chk_wounds CHECK (WoundsPerModel >= 1 AND WoundsPerModel <= 24),
                    CONSTRAINT chk_models CHECK (ModelCount >= 1 AND ModelCount <= 50)
                );
            ");

            // Create indexes for performance
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_weapons_name ON Weapons(Name);");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_units_name ON Units(Name);");

            // Create version table for schema migrations
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS SchemaVersion (
                    Version INTEGER PRIMARY KEY,
                    AppliedAt TEXT NOT NULL
                );
            ");

            ExecuteNonQuery(
                "INSERT OR IGNORE INTO SchemaVersion (Version, AppliedAt) VALUES (@version, @date);",
                new SqliteParameter("@version", 1),
                new SqliteParameter("@date", DateTime.UtcNow.ToString("o"))
            );

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Executes a query with parameters.
    /// REQ-INPUT-004: Parameterized queries prevent SQL injection.
    /// </summary>
    public SqliteDataReader ExecuteQuery(string sql, params SqliteParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query cannot be empty.", nameof(sql));
        }

        if (!IsInitialized)
        {
            throw new InvalidOperationException("Database is not initialized.");
        }

        // REQ-INPUT-004: Verify query uses parameters (basic check)
        if (parameters.Length == 0 && ContainsUserInputPatterns(sql))
        {
            throw new SecurityException(
                "Query appears to use string concatenation instead of parameters. " +
                "Use parameterized queries only (REQ-INPUT-004).");
        }

        try
        {
            var command = _connection!.CreateCommand();
            command.CommandText = sql;

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader();
        }
        catch (SqliteException ex)
        {
            // REQ-ERROR-002: Don't expose database schema in error messages
            throw new InvalidOperationException("Database query failed.", ex);
        }
    }

    /// <summary>
    /// Executes a non-query command with parameters.
    /// REQ-INPUT-004: Parameterized queries prevent SQL injection.
    /// </summary>
    public int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL command cannot be empty.", nameof(sql));
        }

        if (!IsInitialized)
        {
            throw new InvalidOperationException("Database is not initialized.");
        }

        try
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = sql;

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            // REQ-ERROR-002: Don't expose database schema in error messages
            throw new InvalidOperationException("Database command failed.", ex);
        }
    }

    /// <summary>
    /// Executes a scalar query with parameters.
    /// REQ-INPUT-004: Parameterized queries prevent SQL injection.
    /// </summary>
    public object? ExecuteScalar(string sql, params SqliteParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query cannot be empty.", nameof(sql));
        }

        if (!IsInitialized)
        {
            throw new InvalidOperationException("Database is not initialized.");
        }

        try
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = sql;

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteScalar();
        }
        catch (SqliteException ex)
        {
            // REQ-ERROR-002: Don't expose database schema in error messages
            throw new InvalidOperationException("Database query failed.", ex);
        }
    }

    /// <summary>
    /// Begins a transaction for multiple operations.
    /// </summary>
    public SqliteTransaction BeginTransaction()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Database is not initialized.");
        }

        return _connection!.BeginTransaction();
    }

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    public void Close()
    {
        if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
        {
            _connection.Close();
        }
    }

    /// <summary>
    /// Basic check to detect potential SQL injection attempts.
    /// This is a defense-in-depth measure; parameterized queries are the primary defense.
    /// </summary>
    private bool ContainsUserInputPatterns(string sql)
    {
        // Look for common patterns that suggest string concatenation
        string[] suspiciousPatterns = { "'+", "' +", "+ '", "' OR '", "'; DROP", "'; DELETE", "' --" };

        return suspiciousPatterns.Any(pattern =>
            sql.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
