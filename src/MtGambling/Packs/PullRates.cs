using System.Text.Json.Serialization;

namespace MtGambling.Packs;

[method: JsonConstructor]
public record Rates(
    int Common,
    int Uncommon,
    int Rare,
    int Mythic);