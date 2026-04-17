using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MTGallery;
using MTGallery.Configuration;
using MTGallery.PackGeneration;
using MTGallery.Persistence;

/* ---------------------------------------- CONFIGURATION ---------------------------------------- */

var configurationBuilder = new ConfigurationBuilder().AddJsonFile(@"C:\Users\Liam Cox\git\MTGallery\appsettings.json", optional: false);
var configuration =  configurationBuilder.Build();

var outputOptions = configuration.GetSection(nameof(OutputOptions)).Get<OutputOptions>() 
                    ?? throw new NullReferenceException("Failed to load configuration!");

var databaseOptions = configuration.GetSection(nameof(DatabaseConfigurationOptions)).Get<DatabaseConfigurationOptions>() 
                      ?? throw new  NullReferenceException("Failed to load configuration!");

var configuredSetsOptions = configuration.GetSection(nameof(ConfiguredSetsOptions)).Get<ConfiguredSetsOptions>() 
                            ?? throw new NullReferenceException("Failed to load configuration!");

/* ---------------------------------------- DI ---------------------------------------- */

var cache =  new MemoryCache(new MemoryCacheOptions());
var postgreSqlRepository = new PostgreSqlRepository(cache, databaseOptions);
var initializeTask = postgreSqlRepository.InitializeAsync();
var reportGenerator = new ReportGenerator(postgreSqlRepository, configuredSetsOptions, outputOptions);
var packGenerator = new PackGenerationCoordinator(postgreSqlRepository);
await initializeTask;

/* ---------------------------------------- User Interface ---------------------------------------- */

var input = string.Empty;
while (input is not "q")
{
    Console.WriteLine("Choose an option from the following list:");
    Console.WriteLine("1) Generate packs");
    Console.WriteLine("2) Load a commander set");
    Console.WriteLine("3) Generate HTML report");
    Console.WriteLine("T) Truncate pulled cards table");
    Console.WriteLine("q) Quit");
    input = Console.ReadLine();
    if (input is "1") await GeneratePacksInteractiveAsync();
    else if (input is "2") await LoadCommanderSetAsync();
    else if (input is "T") await TruncateDataBaseInteractiveAsync();
    else if (input is "q" or "3")
    {
        await reportGenerator.WriteHtmlReportAsync();
    }
    else Console.WriteLine("Invalid input.");
}

Console.WriteLine("Exiting...");
return; 

/* ---------------------------------------- User Interface Helper Functions ---------------------------------------- */

async Task LoadCommanderSetAsync()
{
    var setCode = string.Empty;
    while (!configuredSetsOptions.ConfiguredCommanderSets.Contains(setCode))
    {
        Console.WriteLine("Enter a configured set code (or enter c to cancel):");
        setCode = Console.ReadLine() ?? string.Empty;
        if (setCode is "c") return;
    }

    var commanderCards = await postgreSqlRepository.GetCardsForSetAsync(setCode);
    await postgreSqlRepository.UpsertCommanderCardsAsync(commanderCards);
}

async Task GeneratePacksInteractiveAsync()
{
    var setCode = string.Empty;
    while (!packGenerator.PullableSets.Contains(setCode))
    {
        Console.WriteLine("Enter a configured set code (or enter c to cancel):");
        setCode = Console.ReadLine() ?? string.Empty;
        if (setCode is "c") return;
    }
    
    Console.WriteLine("How many packs would you like to generate?");
    var numberOfPacks = 0;
    while (!int.TryParse(Console.ReadLine(), out numberOfPacks))
    {
        Console.WriteLine("Please enter a valid natural number!");
    }
    var pulledCards = await packGenerator.GeneratePacksAsync(setCode, numberOfPacks);
    var upsertTask = postgreSqlRepository.UpsertPulledCardsAsync(pulledCards);
    Console.WriteLine("Pulled the following cards:");
    foreach (var (card, count) in pulledCards)
    {
        Console.WriteLine($"{count} {card.Name}");
    }

    await upsertTask;
}

async Task TruncateDataBaseInteractiveAsync()
{
    Console.WriteLine("Are you sure you want to reset the pulled cards table? This action cannot be undone!");
    Console.WriteLine("Enter Y to proceed and any other key to cancel:");

    if (Console.ReadLine() == "Y")
    {
        var truncateTask = postgreSqlRepository.TruncatePulledCardsTable();
        Console.WriteLine("Truncating table!!!");
        await truncateTask;
    }
    else
    {
        Console.WriteLine("Cancelled.");
    }
}