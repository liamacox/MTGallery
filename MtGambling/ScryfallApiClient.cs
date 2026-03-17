using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MtGambling;

public class ScryfallApiClient
{
    private readonly Dictionary<string, JsonArray> _setDataBySetCode = new();
    private readonly string _dataDirectory;
    
    public ScryfallApiClient(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        var files = Directory.GetFiles(dataDirectory);
        foreach (var file in files)
        {
            var setCode = file.Remove(file.LastIndexOf('.'));
            _setDataBySetCode.Add(setCode, new JsonArray(File.ReadAllText(file)));
        }
    }
    
    public JsonArray GetSetData(string setCode)
    {
        if (_setDataBySetCode.TryGetValue(setCode, out var jsonArray)) return jsonArray;
        
        jsonArray = [];
        
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "MtGambling/1.0");

        var queryString = $"https://api.scryfall.com/cards/search?order=rarity&q=set%3A{setCode}";
        var response = client.GetAsync(queryString).Result;
        if (!response.IsSuccessStatusCode) throw new ArgumentException(response.ReasonPhrase);
        var responseJson = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);

        AddCardDataToArray(jsonArray, responseJson.RootElement.GetProperty("data").EnumerateArray());
        
        while (responseJson.RootElement.GetProperty("has_more").GetBoolean())
        {
            response = client.GetAsync(responseJson.RootElement.GetProperty("next_page").GetString()).Result;
            responseJson = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            if (!response.IsSuccessStatusCode) throw new ArgumentException(response.ReasonPhrase);
            AddCardDataToArray(jsonArray, responseJson.RootElement.GetProperty("data").EnumerateArray());
        }
        
        var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };
        File.WriteAllText(
            $"{_dataDirectory}\\{setCode}.json", 
            jsonArray.ToJsonString(jsonOptions)
            );
        
        return jsonArray;
    }

    private static void AddCardDataToArray(JsonArray jsonData, IEnumerable<JsonElement> cards)
    {
        foreach (var cardData in cards) jsonData.Add(cardData);
    }
}