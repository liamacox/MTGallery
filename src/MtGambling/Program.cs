using MtGambling;
using MtGambling.Packs;
using Persistence;

const string dataDirectory = @"C:\Users\Liam Cox\git\MtGambling\SetData";
const string databasePath = @"C:\Users\Liam Cox\git\MtGambling\pulledCards.db";
var client = new ScryfallApiClient(dataDirectory);
var packGenerator = new PackGenerator(client, dataDirectory);

var repository = new SQLiteCardRepository(databasePath);
await repository.InitializeAsync();

foreach (var card in packGenerator.GeneratePack("ecl"))
{
    await repository.UpsertCardAsync(new CardDto(card.OracleId, 1));
}