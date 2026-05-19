using System.Collections.Frozen;
using System.Text.Json;
using MTGallery.Configuration;
using MTGallery.Domain;
using MTGallery.PackGeneration.Generators;

namespace MTGallery.PackGeneration;

public class PackGenerationCoordinator
{
    private readonly FrozenDictionary<string, IPackGenerator> _packGenerators;

    public PackGenerationCoordinator(PackGeneratorFactory factory, ConfiguredSetsOptions configuredSetsOptions)
    {
        var packGenerators = new Dictionary<string, IPackGenerator>();

        foreach (var file in Directory.GetFiles(configuredSetsOptions.PullableSetsDirectory))
        {
            try
            {
                var setConfiguration = JsonSerializer.Deserialize<PackGeneratorConfiguration>(File.ReadAllText(file));
                if (setConfiguration is null)
                {
                    Console.WriteLine($"Warning: failed to deserialize set configuration at {file}.");
                    continue;
                }
                
                if (!packGenerators.TryAdd(setConfiguration.SetCode, factory.GetPackGenerator(setConfiguration)))
                {
                    Console.WriteLine($"Warning: duplicate configuration files found for set {setConfiguration.SetCode}.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Warning: failed to deserialize set configuration at {file}.");
            }
        }

        _packGenerators = packGenerators.ToFrozenDictionary();
    }

    public IReadOnlyCollection<string> PullableSets => _packGenerators.Keys;
    
    public Task<FrozenDictionary<Card, int>> GeneratePacksAsync(string setCode, int numberOfPacks = 1)
    {
        if (!_packGenerators.TryGetValue(setCode, out var generator))
            throw new ArgumentException(message: $"{setCode} is not a configured set!");
        if (numberOfPacks < 1)
            throw new ArgumentException(message: "Invalid number of packs!");

        return generator.GeneratePacksAsync(numberOfPacks);
    }
}