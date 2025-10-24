using Microsoft.Data.Sqlite;

namespace DiceRoller.Core.Services.Database;

/// <summary>
/// Interface for database operations.
/// Manages SQLCipher encrypted database connection and initialization.
/// Implements REQ-DATA-001: AES-256 database encryption via SQLCipher.
/// </summary>
public interface IDatabaseService : IDisposable
{
    /// <summary>
    /// Initializes the database connection with encryption.
    /// Creates the database file if it doesn't exist.
    /// </summary>
    /// <param name="databasePath">Path to the database file.</param>
    /// <param name="encryptionKey">32-byte encryption key for AES-256.</param>
    void Initialize(string databasePath, byte[] encryptionKey);

    /// <summary>
    /// Gets the active database connection.
    /// Throws if database is not initialized.
    /// </summary>
    SqliteConnection Connection { get; }

    /// <summary>
    /// Checks if the database has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Creates the database schema (tables, indexes).
    /// </summary>
    void CreateSchema();

    /// <summary>
    /// Executes a query and returns a data reader.
    /// REQ-INPUT-004: Use parameterized queries only.
    /// </summary>
    SqliteDataReader ExecuteQuery(string sql, params SqliteParameter[] parameters);

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE).
    /// REQ-INPUT-004: Use parameterized queries only.
    /// </summary>
    int ExecuteNonQuery(string sql, params SqliteParameter[] parameters);

    /// <summary>
    /// Executes a scalar query (returns single value).
    /// REQ-INPUT-004: Use parameterized queries only.
    /// </summary>
    object? ExecuteScalar(string sql, params SqliteParameter[] parameters);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    SqliteTransaction BeginTransaction();

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    void Close();
}
