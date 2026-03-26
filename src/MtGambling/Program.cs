using System.Text;
using Microsoft.Extensions.Configuration;
using MtGambling;
using MtGambling.Packs;
using MtGambling.Persistence;

var configurationBuilder = new ConfigurationBuilder().AddJsonFile(@"C:\Users\Liam Cox\git\MtGambling\appsettings.json");;

const string dataDirectory = @"C:\Users\Liam Cox\git\MtGambling\SetData";
const string databasePath = @"C:\Users\Liam Cox\git\MtGambling\pulledCards.db";
const string outputPath = @"C:\Users\Liam Cox\git\MtGambling\output.html";

var client = new ScryfallApiClient(dataDirectory);
var packGenerator = new PackGenerator(client, dataDirectory);

var repository = new SQLiteCardRepository(databasePath);
await repository.InitializeAsync();

foreach (var (card, count) in packGenerator.GeneratePack("ecl"))
{
    await repository.UpsertCardAsync(new CardDto(card.OracleId, count));
}

var stringBuilder = new StringBuilder("""
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

foreach (var cardDto in repository.GetCardsAsync().Result)
{
    var card = client.GetSetData("ecl").Single(card => card.OracleId == cardDto.OracleId);
    
    stringBuilder.AppendLine("<tr>");
    stringBuilder.AppendLine($"<th><img src=\"{card.ImageUri}\" alt=\"{card.Name}\"></th>");
    stringBuilder.AppendLine($"<th><a href=\"{card.ScryfallUri}\">{card.Name}</a></th>");
    stringBuilder.AppendLine($"<th>{card.Rarity.ToString()}</th>");
    stringBuilder.AppendLine($"<th>{cardDto.PullCount}</th>");
    stringBuilder.AppendLine("</tr>");
}

stringBuilder.AppendLine("</table>");
stringBuilder.AppendLine("</body>");
stringBuilder.AppendLine("</html>");

File.WriteAllText(outputPath, stringBuilder.ToString());