using System.Text.Json;
using MtGambling;

var client = new ScryfallApiClient(@"C:\Users\Liam Cox\git\MtGambling\SetData");

client.GetSetData("ecl");
client.GetSetData("blb");