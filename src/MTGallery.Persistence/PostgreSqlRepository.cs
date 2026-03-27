using MTGallery.Configuration;
using Npgsql;

namespace MTGallery.Persistence;

public class PostgreSqlRepository(
    DatabaseConfigurationOptions databaseOptions, 
    ConfiguredSetsOptions configuredSetsOptions)
{
    private readonly string _connectionString = $"Host={databaseOptions.Host}:{databaseOptions.Port};Username={databaseOptions.Username};Password={databaseOptions.Password};Database={databaseOptions.Database}";

    public async Task InitializeAsync()
    {
        await CreatePulledCardsTable();
        await CreatePullRatesTable();
        await CreateSetDataTable();
    }

    private async Task CreateSetDataTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        
        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS set_data (
                                  scryfall_id TEXT PRIMARY KEY,
                                  oracle_id TEXT,
                                  set TEXT NOT NULL,
                                  name TEXT NOT NULL,
                                  rarity TEXT NOT NULL,
                                  scryfall_uri TEXT NOT NULL,
                                  image_uri TEXT NOT NULL
                                  );
                              CREATE INDEX idx_set ON set_data(set);
                              """;
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task Create()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        
        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS set_data (
                                  scryfall_id TEXT PRIMARY KEY,
                                  oracle_id TEXT,
                                  set TEXT NOT NULL,
                                  name TEXT NOT NULL,
                                  rarity TEXT NOT NULL,
                                  scryfall_uri TEXT NOT NULL,
                                  image_uri TEXT NOT NULL
                                  );
                              CREATE INDEX idx_set ON set_data(set);
                              """;
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task CreatePulledCardsTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS set_data (
                              scryfall_id TEXT PRIMARY KEY,
                              oracle_id TEXT,
                              set TEXT NOT NULL,
                              name TEXT NOT NULL,
                              rarity TEXT NOT NULL,
                              scryfall_uri TEXT NOT NULL,
                              image_uri TEXT NOT NULL,
                              count INTEGER DEFAULT 1 NOT NULL
                              );
                              """;
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreatePullRatesTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS pull_rates (
                                  set TEXT PRIMARY KEY,
                                  pull_rates TEXT NOT NULL 
                              );
                              """;
        await command.ExecuteNonQueryAsync();
    }
}