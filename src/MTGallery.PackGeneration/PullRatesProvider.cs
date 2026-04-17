using System.Collections.Frozen;
using System.Text.Json;
using MTGallery.Domain;

namespace MTGallery.PackGeneration;

internal static class PullRatesProvider
{
    internal static IReadOnlyList<PullRates> GetPullRates(string setCode)
    {
        if (!_pullRatesBySetCode.TryGetValue(setCode, out var pullRates))
            throw new ArgumentException($"Unknown set code: {setCode}");
        
        return pullRates;
    }
    
    private static readonly FrozenDictionary<string, IReadOnlyList<PullRates>> _pullRatesBySetCode
        = new Dictionary<string, IReadOnlyList<PullRates>>()
        {
            {"blb", JsonSerializer.Deserialize<List<PullRates>>(_blbRates)!},
            {"ecl", JsonSerializer.Deserialize<List<PullRates>>(_eclRates)!}
        }.ToFrozenDictionary();

    private const string _blbRates = """
                                     [{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":180,"Uncommon":600,"Rare":210,"Mythic":30},{"Common":0,"Uncommon":0,"Rare":838,"Mythic":153},{"Common":604,"Uncommon":308,"Rare":85,"Mythic":21}]
                                     """;

    private const string _eclRates = """
                                     [{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":180,"Uncommon":600,"Rare":210,"Mythic":30},{"Common":0,"Uncommon":0,"Rare":838,"Mythic":153},{"Common":604,"Uncommon":308,"Rare":85,"Mythic":21}]
                                     """;

}