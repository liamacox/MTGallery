using System.Collections.Frozen;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery.PackGeneration;

public class MysticalArchivePackGenerator(string setCode, 
    int specialGuestRateNumerator, 
    int specialGuestRateDenominator,
    int specialGuestCollectorNumberLowerBound,
    int specialGuestCollectorNumberUpperBound,
    IReadOnlyList<PullRates> pullRates,
    PostgreSqlRepository repository) : IPackGenerator
{
    private const int MysticalArchivePull = 13;
    private const string MysticalArchiveSetCode = "soa";
    
    private const int SpecialGuestPull = 7;
    private const string SpecialGuestSetCode = "spg";
    
    public async Task<FrozenDictionary<Card, int>> GeneratePacksAsync(int numberOfPacks = 1)
    {
        var mysticalArchiveCardsTask = repository.GetCardsForSetAsync(MysticalArchiveSetCode);
        var setCardsTask = repository.GetCardsForSetAsync(setCode);
        var specialGuestCardsTask = GetSpecialGuestCardsAsync();
        
        Dictionary<Card, int> pulledCards = [];
        
        var allAvailableCards = (await setCardsTask)
            .Concat(await specialGuestCardsTask)
            .Concat(await mysticalArchiveCardsTask)
            .ToFrozenSet();
        
        foreach (var _ in Enumerable.Range(0, numberOfPacks))
        {
            var cardNumber = 0;
            foreach (var rates in pullRates)
            {
                ++cardNumber;
                
                var availableCards = GetAvailableCards(cardNumber, rates, allAvailableCards);
                
                Random.Shared.Shuffle(availableCards);

                var card = availableCards.ElementAt(Random.Shared.Next(0, availableCards.Length));
                pulledCards.TryGetValue(card, out int count);
                pulledCards[card] = count + 1;
            }
        }

        return pulledCards.ToFrozenDictionary();
    }
    
    private Card[] GetAvailableCards(int cardNumber, PullRates rates, FrozenSet<Card> allAvailableCards)
    {
        if (IsSpecialGuestCard(cardNumber))
            return allAvailableCards.Where(card => card.Set == SpecialGuestSetCode).ToArray();
        
        var draws = GenerateRaritiesList(rates);
        var rarity = draws.ElementAt(Random.Shared.Next(0, draws.Count));
        
        if (cardNumber == MysticalArchivePull)
            return allAvailableCards.Where(card => card.Set == MysticalArchiveSetCode && card.Rarity == rarity).ToArray();
        
        return allAvailableCards.Where(card => card.Rarity == rarity && card.Set == setCode).ToArray();
    }
    
    private bool IsSpecialGuestCard(int cardNumber)
    {
        if (cardNumber != SpecialGuestPull) return false;
        Rarity[] chances = 
        [
            .. Enumerable.Repeat(Rarity.SpecialGuest, specialGuestRateNumerator),
            .. Enumerable.Repeat(Rarity.Common, specialGuestRateDenominator - specialGuestRateNumerator),
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

    private async Task<IEnumerable<Card>> GetSpecialGuestCardsAsync()
        => await repository.GetSpecialGuestCardsInRangeAsync(specialGuestCollectorNumberLowerBound, specialGuestCollectorNumberUpperBound);
}