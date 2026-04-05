using System.Net.Http.Headers;
using System.Text.Json;
using MTGallery.Domain;

namespace MTGallery.Persistence;

public static class ScryfallApiClient
{
    public static async Task<List<Card>> GetSetDataAsync(string setCode)
    {
        List<Card> cards = [];
        
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "MTGallery/1.0");
        
        var queryString = $"https://api.scryfall.com/cards/search?order=rarity&q=set%3A{setCode}";
        JsonDocument responseJson;
        do
        {
            var response = await client.GetAsync(queryString);
            responseJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!response.IsSuccessStatusCode) throw new ArgumentException(response.ReasonPhrase);

            cards.AddRange(
                responseJson.RootElement.GetProperty("data").EnumerateArray()
                    .Where(jsonElement => jsonElement.GetProperty("games").ToString().Contains("paper"))
                    .Select<JsonElement, Card>(Card.BuildCardFromJson));
            
            queryString = responseJson.RootElement.GetProperty("next_page").GetString();
            Thread.Sleep(500);
        } while (responseJson.RootElement.GetProperty("has_more").GetBoolean());
        
        return cards;
    }
}