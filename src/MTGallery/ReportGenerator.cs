using MTGallery.Configuration;
using MTGallery.Persistence;

namespace MTGallery;

public class ReportGenerator(
    PostgreSqlRepository repository,
    ConfiguredSetsOptions configuredSetsOptions,
    OutputOptions outputOptions)
{
    public async Task WriteHtmlReportAsync()
    {
        var cards = repository.GetPulledCardsAsync();
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

        foreach (var (card, count) in await cards)
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

        await File.AppendAllTextAsync(outputOptions.OutputPath, """

                                                                </tbody>
                                                                </table>
                                                                </body>
                                                                </html>

                                                                """);

        await File.AppendAllTextAsync(outputOptions.OutputPath, """
                                                                <script>
                                                                document.querySelectorAll('#cards thead th').forEach((th, index) => {
                                                                  th.style.cursor = index === 0 ? '' : 'pointer';
                                                                  if (index === 0) return;
                                                                  th.addEventListener('click', () => sortTable(index, th));
                                                                });

                                                                document.querySelectorAll('#cards thead th').forEach((th) => {
                                                                  const span = document.createElement('span');
                                                                  span.className = 'sort-indicator';
                                                                  span.textContent = ''; // will be filled by sortTable
                                                                  th.appendChild(span);
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
                                                                  // clear order and indicators on all headers 
                                                                  table.querySelectorAll('th').forEach(h => { 
                                                                    h.dataset.order = ''; 
                                                                    const ind = h.querySelector('.sort-indicator'); 
                                                                    if (ind) ind.textContent = ''; 
                                                                  });
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
                                                                  // set indicator on active header
                                                                  const indicator = th.querySelector('.sort-indicator');
                                                                  if (indicator) indicator.textContent = (current === 'asc') ? '▲' : '▼';

                                                                  // re-apply any filters (if you have that)
                                                                  if (typeof applyCommanderFilter === 'function') applyCommanderFilter();
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
        await File.AppendAllTextAsync(outputOptions.OutputPath, """
                                                                <style>
                                                                  #cards thead th { position: relative; padding-right: 1.2em; }
                                                                  .sort-indicator {
                                                                    position: absolute;
                                                                    right: 6px;
                                                                    top: 50%;
                                                                    transform: translateY(-50%);
                                                                    font-size: 0.8em;
                                                                    pointer-events: none;
                                                                  }
                                                                </style>
                                                                """);
    }
}