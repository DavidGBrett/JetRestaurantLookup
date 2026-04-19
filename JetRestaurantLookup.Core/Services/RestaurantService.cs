using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Mappers;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Services
{
    public class RestaurantService : IRestaurantService
    {
        private const string BaseApiUrl = "https://uk.api.just-eat.io/discovery/uk/restaurants/enriched/bypostcode";
        private const int DefaultCount = 10;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;

        public RestaurantService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Fetches the raw JSON response for restaurants in the given postcode.
        /// </summary>
        private async Task<string> GetRawRestaurantsDataAsync(string postcode)
        {
            var response = await _httpClient.GetAsync($"{BaseApiUrl}/{postcode}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<Restaurant>> GetRestaurantsAsync(string postcode, int count)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(postcode);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            var rawData = await GetRawRestaurantsDataAsync(postcode);

            using var doc = JsonDocument.Parse(rawData);

            JsonElement restaurantsElement = doc.RootElement.GetProperty("restaurants");

            return restaurantsElement
                .EnumerateArray()
                .Take(count)
                .Select(
                    r => JsonSerializer.Deserialize<RestaurantDto>(r, _jsonOptions)
                    ?? throw new InvalidDataException("Unexpected null restaurant")
                )
                .Select(RestaurantMapper.ToModel)
                .ToList();
        }

        public Task<List<Restaurant>> GetRestaurantsAsync(string postcode)
            => GetRestaurantsAsync(postcode, DefaultCount);
    }
}