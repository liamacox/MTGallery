using System.Collections.Frozen;
using System.Text.Json;
using MTGallery.Domain;

namespace MTGallery.PackGeneration;

internal static class PullRatesProvider
{
    private const string BlbRates = """
                                    [{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":180,"Uncommon":600,"Rare":210,"Mythic":30},{"Common":0,"Uncommon":0,"Rare":838,"Mythic":153},{"Common":604,"Uncommon":308,"Rare":85,"Mythic":21}]
                                    """;

    private const string EclRates = """
                                    [{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":180,"Uncommon":600,"Rare":210,"Mythic":30},{"Common":0,"Uncommon":0,"Rare":838,"Mythic":153},{"Common":604,"Uncommon":308,"Rare":85,"Mythic":21}]
                                    """;

    private const string SosRates = """
                                    [{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":1,"Uncommon":0,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":0,"Uncommon":1,"Rare":0,"Mythic":0},{"Common":391,"Uncommon":391,"Rare":200,"Mythic":24},{"Common":0,"Uncommon":0,"Rare":845,"Mythic":154},{"Common":0,"Uncommon":875,"Rare":96,"Mythic":29},{"Common":544,"Uncommon":364,"Rare":77,"Mythic":21}]
                                    """;

    private static readonly FrozenDictionary<string, IReadOnlyList<PullRates>> PullRatesBySetCode
        = new Dictionary<string, IReadOnlyList<PullRates>>
        {
            { "blb", JsonSerializer.Deserialize<List<PullRates>>(BlbRates)! },
            { "ecl", JsonSerializer.Deserialize<List<PullRates>>(EclRates)! },
            { "sos", JsonSerializer.Deserialize<List<PullRates>>(SosRates)! }
        }.ToFrozenDictionary();

    internal static IReadOnlyList<PullRates> GetPullRates(string setCode)
    {
        if (!PullRatesBySetCode.TryGetValue(setCode, out var pullRates))
            throw new ArgumentException($"Unknown set code: {setCode}");

        return pullRates;
    }
}