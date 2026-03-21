using MtGambling;
using MtGambling.Packs;

const string dataDirectory = @"C:\Users\Liam Cox\git\MtGambling\SetData";
var client = new ScryfallApiClient(dataDirectory);
var packGenerator = new PackGenerator(client, dataDirectory);

foreach (var card in packGenerator.GeneratePack("ecl"))
{
    Console.WriteLine(card);
}