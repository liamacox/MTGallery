using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using MTGallery.Configuration;
using MTGallery.Domain;
using Npgsql;

namespace MTGallery.Persistence;

public class PostgreSqlRepository(
    MemoryCache cache,
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
                                  image_uri TEXT NOT NULL,
                                  collector_number INTEGER NOT NULL
                                  );
                              CREATE INDEX IF NOT EXISTS idx_set ON set_data(set);
                              CREATE INDEX IF NOT EXISTS idx_collector_number on set_data(collector_number);
                              """;
        await command.ExecuteNonQueryAsync();
        
        if (configuredSetsOptions.HydrateSetData) await HydrateSetData();
    }

    private async Task HydrateSetData()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();

        await using var truncateCommand = connection.CreateCommand();
        truncateCommand.CommandText = """
                                      TRUNCATE TABLE set_data RESTART IDENTITY;
                                      """;
        await truncateCommand.ExecuteNonQueryAsync();

        foreach (var setCode in configuredSetsOptions.AllConfiguredSets.Append("spg"))
        {
            var cards = await ScryfallApiClient.GetSetDataAsync(setCode);
            
            await using var batch = new NpgsqlBatch(connection);
            foreach (var card in cards)
            {
                var command = new NpgsqlBatchCommand();
                command.CommandText = """
                                      INSERT INTO set_data VALUES (@scryfall_id, @oracle_id, @set, @name, @rarity, @scryfall_uri, @image_uri, @collector_number);
                                      """;
                command.Parameters.AddWithValue("@scryfall_id", card.ScryfallId);
                command.Parameters.AddWithValue("@oracle_id", card.OracleId);
                command.Parameters.AddWithValue("@set", card.Set);
                command.Parameters.AddWithValue("@name", card.Name);
                command.Parameters.AddWithValue("@rarity", card.Rarity.ToString());
                command.Parameters.AddWithValue("@scryfall_uri", card.ScryfallUri);
                command.Parameters.AddWithValue("@image_uri", card.ImageUri);
                command.Parameters.AddWithValue("@collector_number", card.CollectorNumber);
                
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
                              collector_number INTEGER NOT NULL,
                              pull_count INTEGER DEFAULT 1 NOT NULL
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

        var (success, pullRatesList) = GetPullRatesFromCache(setCode);
        if (success) return pullRatesList;
        
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              SELECT pull_rates FROM pull_rates WHERE set = @set LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@set", setCode);

        var pullRatesJson = (await command.ExecuteScalarAsync())?.ToString();
        
        if (pullRatesJson is null) throw new ArgumentException($"{setCode} data could not be found!");
        pullRatesList = JsonSerializer.Deserialize<List<PullRates>>(pullRatesJson);
        if (pullRatesList is null) throw new JsonException($"Could not deserialize pull rates for set {setCode}");
        cache.Set($"{setCode}-pull-rates", pullRatesList);
        
        return pullRatesList;
    }

    private (bool success, List<PullRates> pullRatesJson) GetPullRatesFromCache(string setCode)
    {
        if (!cache.TryGetValue($"{setCode}-pull-rates", out List<PullRates>? pullRates)) 
            return (false, []);
        
        return (pullRates is not null, pullRates ?? []);
    }

    public async Task<HashSet<Card>> GetCardsForSetAsync(string setCode)
    {
        if (!configuredSetsOptions.AllConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");

        var (success, setData) = GetSetDataFromCache(setCode);
        if (success) return setData;
        
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        
        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              SELECT * FROM set_data WHERE set = @set;
                              """;
        command.Parameters.AddWithValue("@set", setCode);

        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var scryfallId = reader.GetString(0);
            var oracleId = reader.GetString(1);
            var set = reader.GetString(2);
            var name =  reader.GetString(3);
            var rarity = Enum.Parse<Rarity>(reader.GetString(4), ignoreCase: true);
            var scryfallUri = reader.GetString(5);
            var imageUri = reader.GetString(6);
            var collectorNumber = reader.GetInt32(7);
            setData.Add(new Card(name, rarity, scryfallId, set, oracleId, scryfallUri, imageUri, collectorNumber));
        }

        cache.Set($"{setCode}-set-data", setData);
        return setData;
    }
    
    private (bool success, HashSet<Card> setData) GetSetDataFromCache(string setCode)
    {
        if (!cache.TryGetValue($"{setCode}-set-data", out HashSet<Card>? setData)) 
            return (false,[]);
        
        return (setData is not null, setData ?? []);
    }

    public async Task UpsertPulledCardsAsync(Dictionary<Card, int> pulledCards)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();

        await using var batch = new NpgsqlBatch(connection);
        
        foreach (var (card, count) in pulledCards) 
        {
            var command = new NpgsqlBatchCommand();
            command.CommandText = """
                                  INSERT INTO pulled_cards (scryfall_id, oracle_id, "set", name, rarity, scryfall_uri, image_uri, collector_number pull_count)
                                  VALUES (@scryfall_id, @oracle_id, @set, @name, @rarity, @scryfall_uri, @image_uri, @collector_number, @pull_count)
                                  ON CONFLICT (scryfall_id)
                                  DO UPDATE SET pull_count = pulled_cards.pull_count + EXCLUDED.pull_count;
                                  """;
            command.Parameters.AddWithValue("@scryfall_id", card.ScryfallId);
            command.Parameters.AddWithValue("@oracle_id", card.OracleId);
            command.Parameters.AddWithValue("@set", card.Set);
            command.Parameters.AddWithValue("@name", card.Name);
            command.Parameters.AddWithValue("@rarity", card.Rarity.ToString());
            command.Parameters.AddWithValue("@scryfall_uri", card.ScryfallUri);
            command.Parameters.AddWithValue("@image_uri", card.ImageUri);
            command.Parameters.AddWithValue("@collector_number", card.CollectorNumber);
            command.Parameters.AddWithValue("@pull_count", count);

            batch.BatchCommands.Add(command);
        }
        await batch.ExecuteNonQueryAsync();
    }
    
    public async Task UpsertCommanderCardsAsync(HashSet<Card> commanderCards)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();

        await using var batch = new NpgsqlBatch(connection);
        
        foreach (var card in commanderCards) 
        {
            var command = new NpgsqlBatchCommand();
            command.CommandText = """
                                  INSERT INTO pulled_cards (scryfall_id, oracle_id, "set", name, rarity, scryfall_uri, image_uri, pull_count)
                                  VALUES (@scryfall_id, @oracle_id, @set, @name, @rarity, @scryfall_uri, @image_uri, @pull_count)
                                  ON CONFLICT (scryfall_id)
                                  DO NOTHING;
                                  """;
            command.Parameters.AddWithValue("@scryfall_id", card.ScryfallId);
            command.Parameters.AddWithValue("@oracle_id", card.OracleId);
            command.Parameters.AddWithValue("@set", card.Set);
            command.Parameters.AddWithValue("@name", card.Name);
            command.Parameters.AddWithValue("@rarity", card.Rarity.ToString());
            command.Parameters.AddWithValue("@scryfall_uri", card.ScryfallUri);
            command.Parameters.AddWithValue("@image_uri", card.ImageUri);
            command.Parameters.AddWithValue("@pull_count", 1);

            batch.BatchCommands.Add(command);
        }
        await batch.ExecuteNonQueryAsync();
    }

    public async Task<List<(Card card, int count)>> GetPulledCardsAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        
        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              SELECT * FROM pulled_cards;
                              """;

        await using var reader = await command.ExecuteReaderAsync();

        List<(Card card, int count)> cards = [];
        while (await reader.ReadAsync())
        {
            var scryfallId = reader.GetString(0);
            var oracleId = reader.GetString(1);
            var set = reader.GetString(2);
            var name =  reader.GetString(3);
            var rarity = Enum.Parse<Rarity>(reader.GetString(4), ignoreCase: true);
            var scryfallUri = reader.GetString(5);
            var imageUri = reader.GetString(6);
            var collectorNumber = reader.GetInt32(7);
            var count = reader.GetInt32(8);
            cards.Add((new Card(name, rarity, scryfallId, set, oracleId, scryfallUri, imageUri, collectorNumber), count));
        }

        return cards;
    }

    public async Task TruncatePulledCardsTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              TRUNCATE TABLE pulled_cards RESTART IDENTITY;
                              """;
        await command.ExecuteNonQueryAsync();
    }
}