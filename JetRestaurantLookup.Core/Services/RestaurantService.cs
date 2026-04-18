using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Mappers;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Services
{
    public class RestaurantService
    {
        private const string BaseApiUrl = "https://uk.api.just-eat.io/discovery/uk/restaurants/enriched/bypostcode";
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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

        public async Task<List<Restaurant>> GetRestaurantsAsync(string postcode, int count)
        {
            var rawData = await GetRawRestaurantsDataAsync(postcode);

            using var doc = JsonDocument.Parse(rawData);

            JsonElement restaurants = doc.RootElement.GetProperty("restaurants");

            List<Restaurant> firstNRestaurants = restaurants
                .EnumerateArray()
                .Take(count)
                .Select(
                    r => JsonSerializer.Deserialize<RestaurantDto>(r, _jsonOptions)
                    ?? throw new InvalidDataException("Unexpected null restaurant")
                )
                .Select(RestaurantMapper.ToModel)
                .ToList();

            return firstNRestaurants;
        }
    }
}