using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MTGallery;
using MTGallery.Configuration;
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
var postgreSqlRepository = new PostgreSqlRepository(cache, databaseOptions, configuredSetsOptions);
var initializeTask = postgreSqlRepository.InitializeAsync();
var packGenerator = new PackGenerator(postgreSqlRepository, configuredSetsOptions);
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
        await WriteHtmlReportAsync();
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
    while (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
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

async Task WriteHtmlReportAsync()
{
    Console.WriteLine("Generating report...");
    await File.WriteAllTextAsync(outputOptions.OutputPath, string.Empty);
    await File.AppendAllTextAsync(outputOptions.OutputPath, """
                                                 <!DOCTYPE html>
                                                 <html>
                                                 <head>
                                                 <title>Cards</title>
                                                 </head>
                                                 <body>
                                                 <button id="toggle-commander-only">Hide commander-only cards</button>
                                                 <table id="cards">
                                                 <thead>
                                                 <tr>
                                                 <th>Card</th>
                                                 <th>Name</th>
                                                 <th>Set</th>
                                                 <th>Rarity</th>
                                                 <th>Count</th>
                                                 <th>Commander</th>
                                                 </tr>
                                                 </thead>
                                                 <tbody>
                                                 """);

    foreach (var (card, count) in await postgreSqlRepository.GetPulledCardsAsync())
    {
        await File.AppendAllTextAsync(outputOptions.OutputPath, $"""

                                                                 <tr>
                                                                 <th><a href="{card.ScryfallUri}" target="_blank" rel="noopener noreferrer"><img src="{card.ImageUri}" alt="{card.Name}"></a></th>
                                                                 <th><a href="{card.ScryfallUri}" target="_blank" rel="noopener noreferrer">{card.Name}</a></th>
                                                                 <th>{card.Set}</th>
                                                                 <th>{card.Rarity.ToString()}</th>
                                                                 <th>{count}</th>
                                                                 <th>{(configuredSetsOptions.ConfiguredCommanderSets.Contains(card.Set) ? "Yes" : "No")}</th>
                                                                 </tr>
                                                                 """);
    }

    await File.AppendAllTextAsync(outputOptions.OutputPath, """

                                                            </tbody>
                                                            </table>
                                                            </body>
                                                            </html>

                                                            """);

    await File.AppendAllTextAsync(outputOptions.OutputPath, """
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
                                                            
                                                            // index of the "Commander" column (0-based). Change if needed.
                                                            const commanderColIndex = 5;
                                                            
                                                            const toggleBtn = document.getElementById('toggle-commander-only');
                                                            let hideCommanderOnly = false;
                                                            
                                                            toggleBtn.addEventListener('click', () => {
                                                              hideCommanderOnly = !hideCommanderOnly;
                                                              toggleBtn.textContent = hideCommanderOnly ? 'Show commander-only cards' : 'Hide commander-only cards';
                                                              applyCommanderFilter();
                                                            });
                                                            
                                                            function applyCommanderFilter() {
                                                              const table = document.getElementById('cards');
                                                              const tbody = table.tBodies[0];
                                                              Array.from(tbody.rows).forEach(row => {
                                                                const val = normalize(getCellValue(row, commanderColIndex));
                                                                if (hideCommanderOnly && val === 'yes') {
                                                                  row.style.display = 'none';
                                                                } else {
                                                                  row.style.display = '';
                                                                }
                                                              });
                                                            }
                                                            </script>
                                                            """);
}