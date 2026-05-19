using System.Text.Json.Serialization;
using MTGallery.Domain;

namespace MTGallery.PackGeneration;

internal class SetWithMysticalArchiveAndSpecialGuestConfiguration : PackGeneratorConfiguration
{
    public required string MysticalArchiveSetCode { get; init; }
}

internal class SetWithSpgPackGeneratorConfiguration : PackGeneratorConfiguration;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "SetType")]
[JsonDerivedType(typeof(SetWithSpgPackGeneratorConfiguration), "SpecialGuest")]
[JsonDerivedType(typeof(SetWithMysticalArchiveAndSpecialGuestConfiguration), "SpecialGuestAndMysticalArchive")]
internal abstract class PackGeneratorConfiguration
{
    public required string SetCode { get; init; }
    public required int SpecialGuestRateNumerator { get; init; }
    public required int SpecialGuestRateDenominator { get; init; }
    public required int SpecialGuestCollectorNumberLowerBound { get; init; }
    public required int SpecialGuestCollectorNumberUpperBound { get; init; }
    public required IReadOnlyList<PullRates> PullRates { get; init; }
}
    