using MTGallery.Configuration;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery;

public class PackGenerator(PostgreSqlRepository repository, ConfiguredSetsOptions configuredSetsOptions)
{
    private const int SpecialGuestPull = 7;
    public async Task<Dictionary<Card, int>> GeneratePacks(string setCode, int numberOfPacks = 1)
    {
        if (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");

        var pullRatesTask = repository.GetPullRatesForSetAsync(setCode);
        var setCardsTask = repository.GetCardsForSetAsync(setCode);
        HashSet<Card> specialGuestCards = [];
        
        Dictionary<Card, int> pulledCards = [];
        
        var generateSpgCard = configuredSetsOptions.SpecialGuestsEnabled && configuredSetsOptions.SpecialGuestRangesBySet.ContainsKey(setCode);
        if (generateSpgCard)
        {
            specialGuestCards = await repository.GetCardsForSetAsync($"spg-{setCode}");
        }
        var pullRates = await pullRatesTask;
        var setCards = await setCardsTask;
        
        foreach (var pack in Enumerable.Range(0, numberOfPacks))
        {
            var pull = 0;
            foreach (var rates in pullRates)
            {
                ++pull;
                var draws = GenerateRaritiesList(rates);
                var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));

                var availableCards = setCards.Where(card => card.Rarity == rarity).ToArray();

                if (generateSpgCard && pull is SpecialGuestPull)
                {
                    var spgRates = configuredSetsOptions.SpecialGuestRatesBySet[setCode].Split(',');
                    var numerator = int.Parse(spgRates[0]);
                    var denominator = int.Parse(spgRates[1]);
                    var specialGuestChances = GenerateSpecialGuestRaritiesList(numerator, denominator);
                    if (specialGuestChances.ElementAt(Random.Shared.Next(0, specialGuestChances.Count)) ==
                        Rarity.SpecialGuest)
                    {
                        availableCards = specialGuestCards.ToArray();
                    }
                }

                Random.Shared.Shuffle(availableCards);

                var card = availableCards.ElementAt(Random.Shared.Next(0, availableCards.Length));
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

    private static List<Rarity> GenerateSpecialGuestRaritiesList(int numerator, int denominator)
    {
        Rarity[] rarities = 
        [
            .. Enumerable.Repeat(Rarity.SpecialGuest, numerator),
            .. Enumerable.Repeat(Rarity.Common, denominator - numerator),
        ];
        
        Random.Shared.Shuffle(rarities);

        return rarities.ToList();
    }
}