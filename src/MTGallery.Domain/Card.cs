using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTGallery.Domain;

[method: JsonConstructor]
public record Card(
    string Name,
    Rarity Rarity,
    string OracleId,
    string ScryfallUri,
    string ImageUri)
{ 
    public static Card BuildCardFromJson(JsonElement cardJson)
    {
        try
        {
            var name = cardJson.GetProperty("name").GetString()!;
            var rarity = Enum.Parse<Rarity>(cardJson.GetProperty("rarity").GetString()!, ignoreCase: true);
            var oracleId = cardJson.GetProperty("oracle_id").GetString()!;
            var scryfallUri = cardJson.GetProperty("scryfall_uri").GetString()!;
            var imageUri = GetImageUri(cardJson);
            return new Card(name, rarity, oracleId, scryfallUri, imageUri);
        }
        catch (Exception)
        {
            throw new ArgumentException($"Could not parse {cardJson.ToString()}");
        }
    }

    private static string GetImageUri(JsonElement cardJson)
    {
        if (cardJson.TryGetProperty("image_uris", out var imageUris))
        {
            return imageUris.GetProperty("small").GetString()!;
        }

        return cardJson.GetProperty("card_faces")
            .EnumerateArray()
            .First()
            .GetProperty("image_uris")
            .GetProperty("small").GetString()!;
    }
};

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Mythic
}