namespace MTGallery.Configuration;

public class ConfiguredSetsOptions
{
    public required HashSet<string> ConfiguredSets { get; init; }
    public required bool HydrateSetData { get; init; }
}