using System.Collections.Frozen;
using MTGallery.Configuration;
using MTGallery.Domain;
using MTGallery.Persistence;

namespace MTGallery.PackGeneration;

public class PackGenerationCoordinator(PostgreSqlRepository repository, ConfiguredSetsOptions configuredSetsOptions)
{
    private readonly FrozenDictionary<string, IPackGenerator> _packGenerators = new Dictionary<string, IPackGenerator>()
    {
        {"blb", new DefaultPackGenerator(
            setCode: "blb", 
            specialGuestRateNumerator: 15,
            specialGuestRateDenominator: 1000,
            specialGuestCollectorNumberLowerBound: 54,
            specialGuestCollectorNumberUpperBound: 63,
            pullRates: PullRatesProvider.GetPullRates(setCode: "blb"),
            repository: repository)},
        {"ecl", new DefaultPackGenerator(
            setCode: "ecl", 
            specialGuestRateNumerator: 1,
            specialGuestRateDenominator: 55,
            specialGuestCollectorNumberLowerBound: 129,
            specialGuestCollectorNumberUpperBound: 148,
            pullRates: PullRatesProvider.GetPullRates(setCode: "ecl"),
            repository: repository)},
    }.ToFrozenDictionary();
    
    public IReadOnlyCollection<string> PullableSets => _packGenerators.Keys;
    
    public Task<FrozenDictionary<Card, int>> GeneratePacksAsync(string setCode, int numberOfPacks = 1)
    {
        if (!_packGenerators.ContainsKey(key: setCode))
            throw new ArgumentException(message: $"{setCode} is not a configured set!");
        if (numberOfPacks < 1)
            throw new ArgumentException(message: "Invalid number of packs!");

        return _packGenerators[key: setCode].GeneratePacksAsync(numberOfPacks: numberOfPacks);
    }
}