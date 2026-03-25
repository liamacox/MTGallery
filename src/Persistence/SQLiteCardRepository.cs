using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;

namespace Persistence;

public class SQLiteCardRepository(string databasePath)
{
    private readonly string _connectionString = $"Data Source={databasePath}";
    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS Cards (
                                  oracle_id TEXT PRIMARY KEY,
                                  pull_count INTEGER DEFAULT 1 NOT NULL 
                              );
                              """;
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpsertCardAsync(CardDto card)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO Cards (oracle_id, pull_count)
                              VALUES (@oracle_id, @pull_count)
                              ON CONFLICT (oracle_id)
                              DO UPDATE SET pull_count = @pull_count + pull_count
                              """;
        command.Parameters.AddWithValue("@oracle_id", card.OracleId);
        command.Parameters.AddWithValue("@pull_count", card.PullCount);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<CardDto>> GetCardsAsync()
    {
        ConcurrentBag<CardDto> cards = [];
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT card.oracle_id, card.pull_count FROM Cards card";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            cards.Add(new CardDto(reader.GetString(0), reader.GetInt32(1)));
        }

        return cards.ToList();
    }
}