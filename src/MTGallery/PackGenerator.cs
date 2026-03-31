using MTGallery.Configuration;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery;

public class PackGenerator(PostgreSqlRepository repository, ConfiguredSetsOptions configuredSetsOptions)
{
    public async Task<Dictionary<Card, int>> GeneratePacks(string setCode, int numberOfPacks = 1)
    {
        if (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");

        var pullRatesTask = repository.GetPullRatesForSetAsync(setCode);
        var setCardsTask = repository.GetCardsForSetAsync(setCode);
        
        Dictionary<Card, int> pulledCards = [];
        var pullRates = await pullRatesTask;
        var setCards = await setCardsTask;
        
        foreach (var pack in Enumerable.Range(0, numberOfPacks))
        {
            foreach (var rates in pullRates)
            {
                var draws = GenerateRaritiesList(rates);
                var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));

                var availableCardsByRarity = setCards.Where(card => card.Rarity == rarity).ToArray();
                Random.Shared.Shuffle(availableCardsByRarity);

                var card = availableCardsByRarity.ElementAt(Random.Shared.Next(0, availableCardsByRarity.Length));
                pulledCards.TryGetValue(card, out int count);
                pulledCards[card] = count + 1;
            }
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
        
        Random.Shared.Shuffle(rarities);

        return rarities.ToList();
    }
}