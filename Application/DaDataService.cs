using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SUPPLY_API;

namespace SUPPLY_API
{
    public class DaDataService
    {
        /// <summary>
        /// Сервис получения данных о компании на сайте dadata
        /// </summary>
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private object token;

        public DaDataService(object token)
        {
            this.token = token;
        }

        public DaDataService(HttpClient httpClient, IOptions<RuTokenSettings> tokenSettings)
        {
            _httpClient = httpClient;
            _token = tokenSettings.Value.Token;
        }

        public async Task<PartyData?> FindPartyAsync(string inn)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/party");

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
}