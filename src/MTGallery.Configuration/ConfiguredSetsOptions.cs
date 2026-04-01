namespace MTGallery.Configuration;

public class ConfiguredSetsOptions
{
    public required HashSet<string> ConfiguredSets { get; init; }
    public required HashSet<string> ConfiguredCommanderSets { get; init; }
    public required bool HydrateSetData { get; init; }
    public HashSet<string> AllConfiguredSets => ConfiguredCommanderSets.Concat(ConfiguredSets).ToHashSet();

}