using System.IO;
using Microsoft.Data.Sqlite;
using MASA.PasswordGenerator.Core.Enums;
using MASA.PasswordGenerator.Core.Interfaces;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Infrastructure.Storage;

public class SqliteHistoryRepository : IHistoryRepository
{
    private readonly ISecureStorage _secureStorage;
    private readonly string _connectionString;
    private readonly ISettingsService _settingsService;

    public SqliteHistoryRepository(ISecureStorage secureStorage, ISettingsService settingsService)
    {
        _secureStorage = secureStorage;
        _settingsService = settingsService;

        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MASA.PasswordGenerator");

        Directory.CreateDirectory(appDataPath);
        string dbPath = Path.Combine(appDataPath, "history.db");
        _connectionString = $"Data Source={dbPath};";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS PasswordHistory (
            Id TEXT PRIMARY KEY,
            EncryptedPassword TEXT NOT NULL,
            Length INTEGER NOT NULL,
            Strength INTEGER NOT NULL,
            EntropyBits REAL NOT NULL,
            CreatedAt TEXT NOT NULL,
            GeneratorType TEXT NOT NULL
        );
        """;
        command.ExecuteNonQuery();
    }

    public async Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry == null) return;

        // Strictly check if history is enabled in Settings
        if (!_settingsService.CurrentSettings.HistoryEnabled)
        {
            return;
        }

        string encrypted = _secureStorage.Protect(entry.Password);

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO PasswordHistory (Id, EncryptedPassword, Length, Strength, EntropyBits, CreatedAt, GeneratorType)
        VALUES ($id, $encrypted, $length, $strength, $entropy, $created, $genType);
        """;
        command.Parameters.AddWithValue("$id", entry.Id);
        command.Parameters.AddWithValue("$encrypted", encrypted);
        command.Parameters.AddWithValue("$length", entry.Length);
        command.Parameters.AddWithValue("$strength", (int)entry.Strength);
        command.Parameters.AddWithValue("$entropy", entry.EntropyBits);
        command.Parameters.AddWithValue("$created", entry.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("$genType", entry.GeneratorType);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<HistoryEntry>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, EncryptedPassword, Length, Strength, EntropyBits, CreatedAt, GeneratorType FROM PasswordHistory ORDER BY CreatedAt DESC;";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string id = reader.GetString(0);
            string encrypted = reader.GetString(1);
            int length = reader.GetInt32(2);
            int strength = reader.GetInt32(3);
            double entropy = reader.GetDouble(4);
            DateTime createdAt = DateTime.Parse(reader.GetString(5));
            string genType = reader.GetString(6);

            string decrypted = _secureStorage.Unprotect(encrypted);

            list.Add(new HistoryEntry
            {
                Id = id,
                Password = decrypted,
                Length = length,
                Strength = (PasswordStrength)strength,
                EntropyBits = entropy,
                CreatedAt = createdAt,
                GeneratorType = genType
            });
        }

        return list;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM PasswordHistory WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM PasswordHistory;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HistoryEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(query)) return all;

        return all.Where(e =>
            e.Password.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.GeneratorType.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Strength.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
}
