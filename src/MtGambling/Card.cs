using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtGambling;

[method: JsonConstructor]
public record Card(
    string Name,
    Rarity Rarity,
    string OracleId,
    string ScryfallUri)
{
    public Card(JsonElement cardJson)
        : this(
            cardJson.GetProperty("name").GetString() ?? throw new ArgumentException($"Could not get property 'name' from {cardJson.ToString()}"),
            Enum.Parse<Rarity>(cardJson.GetProperty("rarity").GetString() ?? throw new ArgumentException($"Could not get property 'rarity' from {cardJson.ToString()}"), true),
            cardJson.GetProperty("oracle_id").GetString() ?? throw new ArgumentException($"Could not get property 'oracle_id' from {cardJson.ToString()}"),
            cardJson.GetProperty("scryfall_uri").GetString() ?? throw new ArgumentException($"Could not get property 'scryfall_uri' from {cardJson.ToString()}"))
    {
    }
};

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Mythic
}