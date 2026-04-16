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
            "blb", 
            15,
            1000,
            54,
            63,
            repository)},
        {"ecl", new DefaultPackGenerator(
            "ecl", 
            1,
            55,
            129,
            148,
            repository)},
    }.ToFrozenDictionary();
    
    public Task<FrozenDictionary<Card, int>> GeneratePacksAsync(string setCode, int numberOfPacks = 1)
    {
        if (!_packGenerators.ContainsKey(setCode))
            throw new ArgumentException($"{setCode} is not a configured set!");
        if (numberOfPacks < 1)
            throw new ArgumentException("Invalid number of packs!");

        return _packGenerators[setCode].GeneratePacksAsync(numberOfPacks);
    }
}