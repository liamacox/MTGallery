using System.Text.Json.Serialization;

namespace MTGallery.Domain;

[method: JsonConstructor]
public record PullRates(
    int Common,
    int Uncommon,
    int Rare,
    int Mythic);