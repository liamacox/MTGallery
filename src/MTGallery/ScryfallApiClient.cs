using System.Net.Http.Headers;
using System.Text.Json;
using MTGallery.Domain;

namespace MTGallery;

public class ScryfallApiClient
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly Dictionary<string, List<Card>> _setDataBySetCode = new();
    private readonly string _dataDirectory;
    
    public ScryfallApiClient(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        var files = Directory.GetFiles(dataDirectory).Where(file => Path.GetExtension(file) == ".json");
        foreach (var file in files)
        {
            var setCode = file.Remove(file.LastIndexOf('.')).Split('\\').Last();
            var setData = JsonSerializer.Deserialize<List<Card>>(File.ReadAllText(file));
            if (setData is null) throw new ArgumentException($"Failed to read file {file}.");
            _setDataBySetCode.Add(setCode, setData);
        }
    }
    
    public List<Card> GetSetData(string setCode)
    {
        if (_setDataBySetCode.TryGetValue(setCode, out var cards)) return cards;
        
        cards = [];
        
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "MTGallery/1.0");

        var queryString = $"https://api.scryfall.com/cards/search?order=rarity&q=set%3A{setCode}";
        var response = client.GetAsync(queryString).Result;
        if (!response.IsSuccessStatusCode) throw new ArgumentException(response.ReasonPhrase);
        var responseJson = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
        
        cards.AddRange(
            responseJson.RootElement.GetProperty("data").EnumerateArray()
            .Select<JsonElement, Card>(Card.BuildCardFromJson));
        
        while (responseJson.RootElement.GetProperty("has_more").GetBoolean())
        {
            Thread.Sleep(500);
            response = client.GetAsync(responseJson.RootElement.GetProperty("next_page").GetString()).Result;
            responseJson = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            if (!response.IsSuccessStatusCode) throw new ArgumentException(response.ReasonPhrase);
            
            cards.AddRange(
                responseJson.RootElement.GetProperty("data").EnumerateArray()
                    .Select<JsonElement, Card>(Card.BuildCardFromJson));
        }
        
        File.WriteAllText(
            $"{_dataDirectory}\\{setCode}.json", 
            JsonSerializer.Serialize(cards, _jsonOptions)
            );
        
        _setDataBySetCode.Add(setCode, cards);
        return cards;
    }
}