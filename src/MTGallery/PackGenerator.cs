using System.Text.Json;
using MTGallery.Domain;

namespace MTGallery;

public class PackGenerator
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {AllowTrailingCommas = true};
    private readonly ScryfallApiClient _scryfallApiClient;
    private readonly Dictionary<string, List<PullRates>> _pullRatesBySet;

    public PackGenerator(ScryfallApiClient scryfallApiClient, string dataDirectory)
    {
        _scryfallApiClient = scryfallApiClient;
        _pullRatesBySet = [];
        
        var files = Directory.GetFiles(dataDirectory).Where(file => Path.GetExtension(file) == ".rates");
        foreach (var file in files)
        {
            var setCode = file.Remove(file.LastIndexOf('.')).Split('\\').Last();
            var pullRates = JsonSerializer.Deserialize<List<PullRates>>(File.ReadAllText(file), _jsonSerializerOptions);
            if (pullRates is null) throw new JsonException($"Could not load pull rates for file {file}");
            _pullRatesBySet.Add(setCode, pullRates);
        }
    }
    
    public Dictionary<Card, int> GeneratePack(string setCode)
    {
        if (!_pullRatesBySet.TryGetValue(setCode, out var pullRates))
            throw new ArgumentException($"Could not find the pull rates for set {setCode}");

        Dictionary<Card, int> pulledCards = [];
        foreach (var rates in pullRates)
        {
            var draws = GenerateRaritiesList(rates);
            var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));

            var cards = _scryfallApiClient
                .GetSetData(setCode)
                .Where(card => card.Rarity == rarity)
                .ToList();

            var card = cards.ElementAt(Random.Shared.Next(0, cards.Count));
            pulledCards.TryGetValue(card, out int count);
            pulledCards[card] = count + 1;
        }
        
        return pulledCards;
    }

    private static List<Rarity> GenerateRaritiesList(PullRates pullRates)
    {
        Rarity[] rarities = 
        [
            .. Enumerable.Repeat(Rarity.Common, pullRates.Common),
            .. Enumerable.Repeat(Rarity.Uncommon, pullRates.Uncommon),
            .. Enumerable.Repeat(Rarity.Rare, pullRates.Rare),
            .. Enumerable.Repeat(Rarity.Mythic, pullRates.Mythic)
        ];
        
        Random.Shared.Shuffle(rarities.ToArray());

        return rarities.ToList();
    }
}