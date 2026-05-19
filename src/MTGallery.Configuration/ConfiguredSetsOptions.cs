namespace MTGallery.Configuration;

public class ConfiguredSetsOptions
{
    public required HashSet<string> ConfiguredCommanderSets { get; init; }
    public required string PullableSetsDirectory { get; init; }
}