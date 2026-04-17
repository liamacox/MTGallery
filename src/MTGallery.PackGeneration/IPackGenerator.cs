using System.Collections.Frozen;
using MTGallery.Domain;

namespace MTGallery.PackGeneration;

internal interface IPackGenerator
{
    internal Task<FrozenDictionary<Card, int>> GeneratePacksAsync(int numberOfPacks = 1);
}