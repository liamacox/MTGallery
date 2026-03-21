using System.Text.Json;

namespace MtGambling.Packs;

public class PackGenerator
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {AllowTrailingCommas = true};
    private readonly ScryfallApiClient _scryfallApiClient;
    private readonly Dictionary<string, List<Rates>> _pullRatesBySet;

    public PackGenerator(ScryfallApiClient scryfallApiClient, string dataDirectory)
    {
        _scryfallApiClient = scryfallApiClient;
        _pullRatesBySet = [];
        
        var files = Directory.GetFiles(dataDirectory).Where(file => Path.GetExtension(file) == ".rates");
        foreach (var file in files)
        {
            var setCode = file.Remove(file.LastIndexOf('.')).Split('\\').Last();
            var pullRates = JsonSerializer.Deserialize<List<Rates>>(File.ReadAllText(file), _jsonSerializerOptions);
            if (pullRates is null) throw new JsonException($"Could not load pull rates for file {file}");
            _pullRatesBySet.Add(setCode, pullRates);
        }
    }
    
    public List<Card> GeneratePack(string setCode)
    {
        if (!_pullRatesBySet.TryGetValue(setCode, out var pullRates))
            throw new ArgumentException($"Could not find the pull rates for set {setCode}");

        List<Card> pulledCards = [];
        foreach (var rates in pullRates)
        {
            var draws = GenerateRaritiesList(rates);
            var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));

            var cards = _scryfallApiClient
                .GetSetData(setCode)
                .Where(set => set.Rarity == rarity)
                .ToList();

            pulledCards.Add(cards.ElementAt(Random.Shared.Next(0, cards.Count)));
        }
        
        return pulledCards;
    }

    private static List<Rarity> GenerateRaritiesList(Rates rates)
    {
        Rarity[] rarities = 
        [
            .. Enumerable.Repeat(Rarity.Common, rates.Common),
            .. Enumerable.Repeat(Rarity.Uncommon, rates.Uncommon),
            .. Enumerable.Repeat(Rarity.Rare, rates.Rare),
            .. Enumerable.Repeat(Rarity.Mythic, rates.Mythic)
        ];
        
        Random.Shared.Shuffle(rarities.ToArray());

        return rarities.ToList();
    }
}