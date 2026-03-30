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
var initializeTask = postgreSqlRepository.InitializeAsync();
var packGenerator = new PackGenerator(postgreSqlRepository, configuredSetsOptions);
await initializeTask;

/* ---------------------------------------- User Interface ---------------------------------------- */

var input = string.Empty;
while (input != "q")
{
    Console.WriteLine("Choose an option from the following list:");
    Console.WriteLine("1) Generate packs");
    Console.WriteLine("2) Truncate pulled cards table");
    Console.WriteLine("q) Quit");
    input = Console.ReadLine();
    if (input == "1") await GeneratePacksInteractiveAsync();
    else if (input == "2") await TruncateDataBaseInteractiveAsync();
    else if (input == "q") Console.WriteLine("Exiting, generating report...");
    else Console.WriteLine("Invalid input.");
}

/* ---------------------------------------- Generate Report ---------------------------------------- */

File.WriteAllText(outputOptions.OutputPath, string.Empty);
File.AppendAllText(outputOptions.OutputPath, """
                                            <!DOCTYPE html>
                                            <html>
                                            <head>
                                            <title>Cards</title>
                                            </head>
                                            <body>
                                            <table id="cards">
                                            <thead>
                                            <tr>
                                            <th>Card</th>
                                            <th>Name</th>
                                            <th>Set</th>
                                            <th>Rarity</th>
                                            <th>Count</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                            """);

foreach (var (card, count) in await postgreSqlRepository.GetPulledCardsAsync())
{
    File.AppendAllText(outputOptions.OutputPath,$"""
                                                 
                                                 <tr>
                                                 <th><img src="{card.ImageUri}" alt="{card.Name}"></th>
                                                 <th><a href="{card.ScryfallUri}">{card.Name}</a></th>
                                                 <th>{card.Set}</th>
                                                 <th>{card.Rarity.ToString()}</th>
                                                 <th>{count}</th>
                                                 </tr>
                                                 """);
}

File.AppendAllText(outputOptions.OutputPath, """
                                             
                                             </tbody>
                                             </table>
                                             </body>
                                             </html>
                                             
                                             """);
                                             
File.AppendAllText(outputOptions.OutputPath, """
                                             <script>
                                             document.querySelectorAll('#cards thead th').forEach((th, index) => {
                                               th.style.cursor = 'pointer';
                                               th.addEventListener('click', () => sortTable(index, th));
                                             });
                                             
                                             function getCellValue(row, idx) {
                                               const cell = row.children[idx];
                                               // prefer textual content, fall back to image alt or link text
                                               const link = cell.querySelector('a');
                                               if (link) return link.textContent.trim();
                                               const img = cell.querySelector('img');
                                               if (img && img.alt) return img.alt.trim();
                                               return cell.textContent.trim();
                                             }
                                             
                                             function isNumeric(n){ return !isNaN(parseFloat(n)) && isFinite(n); }
                                             
                                             const rarityRank = {
                                               'common': 1,
                                               'uncommon': 2,
                                               'rare': 3,
                                               'mythic': 4
                                             };
                                             
                                             function normalize(s){
                                               return String(s || '').trim().replace(/\s+/g,' ').toLowerCase();
                                             }
                                             
                                             function sortTable(colIndex, th) {
                                               const table = document.getElementById('cards');
                                               const tbody = table.tBodies[0];
                                               const rows = Array.from(tbody.rows);
                                               const current = th.dataset.order === 'asc' ? 'desc' : 'asc';
                                               table.querySelectorAll('th').forEach(h=>h.dataset.order='');
                                               th.dataset.order = current;
                                             
                                               rows.sort((a,b) => {
                                                 let A = getCellValue(a, colIndex);
                                                 let B = getCellValue(b, colIndex);
                                             
                                                 // If sorting the Rarity column, use defined rank order
                                                 if (colIndex === 3) {
                                                   const ra = rarityRank[normalize(A)] || 0;
                                                   const rb = rarityRank[normalize(B)] || 0;
                                                   return (ra - rb) * (current === 'asc' ? 1 : -1);
                                                 }
                                             
                                                 // numeric compare if both numeric
                                                 if (isNumeric(A) && isNumeric(B)) {
                                                   return (parseFloat(A) - parseFloat(B)) * (current === 'asc' ? 1 : -1);
                                                 }
                                             
                                                 // case-insensitive string
                                                 A = A.toLowerCase();
                                                 B = B.toLowerCase();
                                                 return A < B ? (current === 'asc' ? -1 : 1) : (A > B ? (current === 'asc' ? 1 : -1) : 0);
                                               });
                                             
                                               rows.forEach(r => tbody.appendChild(r));
                                             }
                                             
                                             // Default sort by "Set" column (index 2) ascending on load
                                             document.addEventListener('DOMContentLoaded', () => {
                                               const setTh = document.querySelectorAll('#cards thead th')[2];
                                               // ensure it's marked as asc so the first click toggles to desc
                                               setTh.dataset.order = 'desc'; // set opposite so sortTable toggles to 'asc'
                                               sortTable(2, setTh);
                                             });
                                             </script>
                                             """);
return;

/* ---------------------------------------- User Interface Helper Functions ---------------------------------------- */

async Task GeneratePacksInteractiveAsync()
{
    var setCode = string.Empty;
    while (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
    {
        Console.WriteLine("Enter a configured set code:");
        setCode = Console.ReadLine() ?? string.Empty;
    }
    
    Console.WriteLine("How many packs would you like to generate?");
    int numberOfPacks = 0;
    while (!int.TryParse(Console.ReadLine(), out numberOfPacks))
    {
        Console.WriteLine("Please enter a valid natural number!");
    }
    var pulledCards = await packGenerator.GeneratePacks(setCode, numberOfPacks);
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