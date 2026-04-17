using System.Collections.Frozen;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery.PackGeneration;

public class PackGenerationCoordinator(PostgreSqlRepository repository)
{
    private readonly FrozenDictionary<string, IPackGenerator> _packGenerators = new Dictionary<string, IPackGenerator>()
    {
        {"blb", new DefaultPackGenerator(
            setCode: "blb", 
            specialGuestRateNumerator: 15,
            specialGuestRateDenominator: 1000,
            specialGuestCollectorNumberLowerBound: 54,
            specialGuestCollectorNumberUpperBound: 63,
            pullRates: PullRatesProvider.GetPullRates("blb"),
            repository)},
        {"ecl", new DefaultPackGenerator(
            setCode: "ecl", 
            specialGuestRateNumerator: 1,
            specialGuestRateDenominator: 55,
            specialGuestCollectorNumberLowerBound: 129,
            specialGuestCollectorNumberUpperBound: 148,
            pullRates: PullRatesProvider.GetPullRates("ecl"),
            repository)},
        {"sos", new MysticalArchivePackGenerator(
            setCode: "sos",
            specialGuestRateNumerator: 1,
            specialGuestRateDenominator: 55,
            specialGuestCollectorNumberLowerBound: 149,
            specialGuestCollectorNumberUpperBound: 158,
            pullRates: PullRatesProvider.GetPullRates("sos"),
            repository)},
    }.ToFrozenDictionary();
    
    public IReadOnlyCollection<string> PullableSets => _packGenerators.Keys;
    
    public Task<FrozenDictionary<Card, int>> GeneratePacksAsync(string setCode, int numberOfPacks = 1)
    {
        if (!_packGenerators.ContainsKey(setCode))
            throw new ArgumentException(message: $"{setCode} is not a configured set!");
        if (numberOfPacks < 1)
            throw new ArgumentException(message: "Invalid number of packs!");

        return _packGenerators[setCode].GeneratePacksAsync(numberOfPacks);
    }
}