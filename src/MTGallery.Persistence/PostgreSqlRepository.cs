using MTGallery.Configuration;
using Npgsql;

namespace MTGallery.Persistence;

public class PostgreSqlRepository(DatabaseConfigurationOptions options)
{
    private readonly string _connectionString = $"Host={options.Host}:{options.Port};Username={options.Username};Password={options.Password};Database={options.Database}";

    public async Task InitializeAsync()
    {
        await CreatePulledCardsTable();
        await CreatePullRatesTable();
    }

    private async Task CreatePulledCardsTable()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS pulled_cards (
                                  oracle_id TEXT PRIMARY KEY,
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
                                  pull_rates TEXT NULL 
                              );
                              """;
        await command.ExecuteNonQueryAsync();
    }
}