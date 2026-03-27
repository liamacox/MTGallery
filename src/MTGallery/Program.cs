using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MTGallery;
using MTGallery.Configuration;
using MTGallery.Persistence;

/* ---------------------------------------- CONFIGURATION ---------------------------------------- */

var configurationBuilder = new ConfigurationBuilder().AddJsonFile(@"C:\Users\Liam Cox\git\MTGallery\appsettings.json", optional: false);
var configuration =  configurationBuilder.Build();

var outputOptions = new OutputOptions();
configuration.GetSection(nameof(OutputOptions)).Bind(outputOptions);
var databaseOptions = new DatabaseConfigurationOptions();
configuration.GetSection(nameof(DatabaseConfigurationOptions)).Bind(databaseOptions);
var configuredSetsOptions = new ConfiguredSetsOptions();
configuration.GetSection(nameof(ConfiguredSetsOptions)).Bind(configuredSetsOptions);

/* ---------------------------------------- DI ---------------------------------------- */

var cache =  new MemoryCache(new MemoryCacheOptions());
var client = new ScryfallApiClient();
var postgreSqlRepository = new PostgreSqlRepository(client, cache, databaseOptions, configuredSetsOptions);
await postgreSqlRepository.InitializeAsync();
var packGenerator = new PackGenerator(postgreSqlRepository, configuredSetsOptions);

/* ---------------------------------------- Execution ---------------------------------------- */

var pulledCards = await packGenerator.GeneratePack("blb");
await postgreSqlRepository.UpsertPulledCardsAsync(pulledCards);
File.WriteAllText(outputOptions.OutputPath, string.Empty);
File.AppendAllText(outputOptions.OutputPath, """
                                            <!DOCTYPE html>
                                            <html>
                                            <head>
                                            <title>Cards</title>
                                            </head>
                                            <body>
                                            <table>
                                            <tr>
                                            <th>Card</th>
                                            <th>Name</th>
                                            <th>Rarity</th>
                                            <th>Count</th>
                                            </tr>

                                            """);

foreach (var (card, count) in await postgreSqlRepository.GetPulledCardsAsync())
{
    File.AppendAllText(outputOptions.OutputPath,$"""
                                                 <tr>
                                                 <th><img src="{card.ImageUri}" alt="{card.Name}"></th>
                                                 <th><a href="{card.ScryfallUri}">{card.Name}</a></th>
                                                 <th>{card.Rarity.ToString()}</th>
                                                 <th>{count}</th>
                                                 </tr>
                                                 """);
}

File.AppendAllText(outputOptions.OutputPath, """
                                             </table>
                                             </body>
                                             </html>
                                             """);