using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


public class DaDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public DaDataService(string token)
    {
        _httpClient = new HttpClient();
        _token = token;
    }

    public async Task<PartyData?> FindPartyAsync(string inn)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/party");

        request.Headers.Authorization = new AuthenticationHeaderValue("Token", _token);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { query = inn }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DaDataPartyResponse>(content);

        return result?.suggestions?.FirstOrDefault()?.data;
    }
}

// Вспомогательные классы
public class DaDataPartyResponse
{
    public List<DaDataSuggestion>? suggestions { get; set; }
}

public class DaDataSuggestion
{
    public PartyData? data { get; set; }
}

public class PartyData
{
    public string? inn { get; set; }
    public NameData? name { get; set; }
    public AddressData? address { get; set; }
}

public class NameData
{
    public string? full_with_opf { get; set; }
    public string? short_with_opf { get; set; }
}

public class AddressData
{
    public string? value { get; set; }
}
