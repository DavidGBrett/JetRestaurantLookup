using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace JetRestaurantLookup.Core.Services
{
    public class RestaurantService
    {
        private const string BaseApiUrl = "https://uk.api.just-eat.io/discovery/uk/restaurants/enriched/bypostcode";

        private static readonly HttpClient _httpClient = new();

        public RestaurantService()
        {
            
        }

        private async Task<string> GetRawRestaurantsDataAsync(string postcode)
        {
            var response = await _httpClient.GetAsync($"{BaseApiUrl}/{postcode}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async Task<List<string>> GetNRestaurantsAsync(string postcode, int n)
        {
            var rawData = await GetRawRestaurantsDataAsync(postcode);

            using var doc = JsonDocument.Parse(rawData);

            JsonElement restaurants = doc.RootElement.GetProperty("restaurants");

            List<string> firstNRestaurants = restaurants
                .EnumerateArray()
                .Take(n)
                .Select(r => r.ToString())
                .ToList();

            return firstNRestaurants;
        }
    }
}