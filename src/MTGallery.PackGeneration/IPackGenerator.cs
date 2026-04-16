using System.Collections.Frozen;
using MTGallery.Domain;

namespace MTGallery.PackGeneration;

internal interface IPackGenerator
{
    public Task<FrozenDictionary<Card, int>> GeneratePacksAsync(int numberOfPacks = 1);
}