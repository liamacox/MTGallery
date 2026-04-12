using System.Collections.Frozen;
using MTGallery.Configuration;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery;

public class PackGenerator(PostgreSqlRepository repository, ConfiguredSetsOptions configuredSetsOptions)
{
    private const int SpecialGuestPull = 7;
    public async Task<FrozenDictionary<Card, int>> GeneratePacks(string setCode, int numberOfPacks = 1)
    {
        if (!configuredSetsOptions.ConfiguredSets.Contains(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");

        var pullRatesTask = repository.GetPullRatesForSetAsync(setCode);
        var setCardsTask = repository.GetCardsForSetAsync(setCode);
        var specialGuestCardsTask = GetSpecialGuestCardsAsync(setCode);
        
        Dictionary<Card, int> pulledCards = [];
        
        var pullRates = await pullRatesTask;
        var allAvailableCards = (await setCardsTask).Concat(await specialGuestCardsTask).ToFrozenSet();
        
        foreach (var pack in Enumerable.Range(0, numberOfPacks))
        {
            var cardNumber = 0;
            foreach (var rates in pullRates)
            {
                ++cardNumber;
                
                var availableCards = GetAvailableCards(setCode, cardNumber, rates, allAvailableCards);
                
                Random.Shared.Shuffle(availableCards);

                var card = availableCards.ElementAt(Random.Shared.Next(0, availableCards.Length));
                pulledCards.TryGetValue(card, out int count);
                pulledCards[card] = count + 1;
            }
        }

        return pulledCards.ToFrozenDictionary();
    }

    private const string SpecialGuestSetCode = "spg";
    private Card[] GetAvailableCards(string setCode, int cardNumber, PullRates rates, FrozenSet<Card> allAvailableCards)
    {
        if (IsSpecialGuestCard(setCode, cardNumber))
            return allAvailableCards.Where(card => card.Set == SpecialGuestSetCode).ToArray();
        
        var draws = GenerateRaritiesList(rates);
        var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));
        
        return allAvailableCards.Where(card => card.Rarity == rarity && card.Set == setCode).ToArray();
    }
    
    private bool IsSpecialGuestCard(string setCode, int cardNumber)
    {
        if (!configuredSetsOptions.SpecialGuestsEnabled || cardNumber is not SpecialGuestPull) return false;
        
        var spgRates = configuredSetsOptions.SpecialGuestRatesBySet[setCode].Split(',');
        var numerator = int.Parse(spgRates[0]);
        var denominator = int.Parse(spgRates[1]);
        
        Rarity[] chances = 
        [
            .. Enumerable.Repeat(Rarity.SpecialGuest, numerator),
            .. Enumerable.Repeat(Rarity.Common, denominator - numerator),
        ];
        
        Random.Shared.Shuffle(chances);

        return chances.ElementAt(Random.Shared.Next(0, chances.Length)) == Rarity.SpecialGuest;
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

    private async Task<IEnumerable<Card>> GetSpecialGuestCardsAsync(string setCode)
    {
        var generateSpgCards = configuredSetsOptions.SpecialGuestsEnabled 
                               && configuredSetsOptions.SpecialGuestRangesBySet.ContainsKey(setCode)
                               && configuredSetsOptions.SpecialGuestRatesBySet.ContainsKey(setCode);

        if (!generateSpgCards) return [];

        var range = configuredSetsOptions.SpecialGuestRangesBySet[setCode].Split(',');
        var lowerBound = int.Parse(range[0]);
        var upperBound = int.Parse(range[1]);

        var specialGuestCards = await repository.GetSpecialGuestCardsInRangeAsync(lowerBound, upperBound);

        return specialGuestCards;
    }
}