namespace MTGallery.Configuration;

public class ConfiguredSetsOptions
{
    public required HashSet<string> ConfiguredSets { get; init; }
    public required HashSet<string> ConfiguredCommanderSets { get; init; }
    public required bool HydrateSetData { get; init; }
    public required bool SpecialGuestsEnabled { get; init; }
    public required Dictionary<string, string> SpecialGuestRangesBySet { get; init; }
    public required Dictionary<string, string> SpecialGuestRatesBySet { get; init; }
    public HashSet<string> AllConfiguredSets => ConfiguredCommanderSets.Concat(ConfiguredSets).ToHashSet();
}