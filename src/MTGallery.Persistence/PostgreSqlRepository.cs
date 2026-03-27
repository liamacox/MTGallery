using System.Text.Json;
using MTGallery.Configuration;
using MTGallery.Domain;
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
                              CREATE INDEX IF NOT EXISTS idx_set ON set_data(set);
                              """;
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task CreatePulledCardsTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS pulled_cards (
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

    public async Task<List<PullRates>> GetPullRatesForSetAsync(string setCode)
    {
        if (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");
        
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              SELECT pull_rates FROM pull_rates WHERE set = @set LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@set", setCode);

        var pullRatesJson = (await command.ExecuteScalarAsync())?.ToString();
        if (pullRatesJson is null) throw new ArgumentException($"{setCode} data could not be found!");
        
        var pullRates = JsonSerializer.Deserialize<List<PullRates>>(pullRatesJson);
        if (pullRates is null) throw new JsonException($"Could not deserialize pull rates for set {setCode}");

        return pullRates;
    } 
}