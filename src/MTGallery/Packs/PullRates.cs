using System.Text.Json.Serialization;

namespace MTGallery.Packs;

[method: JsonConstructor]
public record Rates(
    int Common,
    int Uncommon,
    int Rare,
    int Mythic);