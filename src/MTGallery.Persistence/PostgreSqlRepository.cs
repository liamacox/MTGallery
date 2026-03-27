using System.Text.Json;
using MTGallery.Configuration;
using MTGallery.Domain;
using Npgsql;

namespace MTGallery.Persistence;

public class PostgreSqlRepository(
    ScryfallApiClient scryfallApiClient,
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
        
        if (configuredSetsOptions.HydrateSetData) await HydrateSetData();
    }

    private async Task HydrateSetData()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var truncateCommand = connection.CreateCommand();
        truncateCommand.CommandText = """
                                      TRUNCATE TABLE set_data RESTART IDENTITY;
                                      """;
        await truncateCommand.ExecuteNonQueryAsync();

        foreach (var setCode in configuredSetsOptions.ConfiguredSets)
        {
            var cards = scryfallApiClient.GetSetData(setCode);
            
            await using var batch = new NpgsqlBatch(connection);
            foreach (var card in cards)
            {
                var command = new NpgsqlBatchCommand();
                command.CommandText = """
                                      INSERT INTO set_data VALUES (@scryfall_id, @oracle_id, @set, @name, @rarity, @scryfall_uri, @image_uri);
                                      """;
                command.Parameters.AddWithValue("@scryfall_id", card.ScryfallId);
                command.Parameters.AddWithValue("@oracle_id", card.OracleId);
                command.Parameters.AddWithValue("@set", setCode);
                command.Parameters.AddWithValue("@name", card.Name);
                command.Parameters.AddWithValue("@rarity", card.Rarity.ToString());
                command.Parameters.AddWithValue("@scryfall_uri", card.ScryfallUri);
                command.Parameters.AddWithValue("@image_uri", card.ImageUri);
                
                batch.BatchCommands.Add(command);
            }

            await batch.ExecuteNonQueryAsync();
        }
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

    public async Task<HashSet<Card>> GetCardsForSetAsync(string setCode)
    {
        if (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");
        
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        
        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              SELECT * FROM set_data WHERE set = @set;
                              """;
        command.Parameters.AddWithValue("@set", setCode);

        await using var reader = await command.ExecuteReaderAsync();

        HashSet<Card> cards = [];
        while (await reader.ReadAsync())
        {
            var scryfallId = reader.GetString(0);
            var oracleId = reader.GetString(1);
            var set = reader.GetString(2);
            var name =  reader.GetString(3);
            var rarity = Enum.Parse<Rarity>(reader.GetString(4), ignoreCase: true);
            var scryfallUri = reader.GetString(5);
            var imageUri = reader.GetString(6);
            cards.Add(new Card(name, rarity, scryfallId, set, oracleId, scryfallUri, imageUri));
        }

        return cards;
    }
}