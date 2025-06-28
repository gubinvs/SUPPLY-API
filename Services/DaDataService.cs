using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;


namespace SUPPLY_API
{
    public class DaDataService
    {
        /// <summary>
        /// Сервис получения данных о компании на сайте dadata
        /// </summary>
        private readonly HttpClient _httpClient;
        private readonly string _token;


        public DaDataService(HttpClient httpClient, IOptions<RuTokenSettings> tokenSettings)
        {
            _httpClient = httpClient;
            _token = tokenSettings.Value.Token;

            if (string.IsNullOrWhiteSpace(_token))
                throw new Exception("DaData token is missing!");
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
}