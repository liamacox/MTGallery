using System.Text.Json.Serialization;

namespace MTGallery.Domain;

[method: JsonConstructor]
public record Rates(
    int Common,
    int Uncommon,
    int Rare,
    int Mythic);