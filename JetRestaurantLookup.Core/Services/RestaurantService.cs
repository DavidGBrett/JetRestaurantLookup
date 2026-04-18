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
        private const int DefaultCount = 10;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Fetches the raw JSON response for restaurants in the given postcode.
        /// </summary>
        private async Task<string> GetRawRestaurantsDataAsync(string postcode)
        {
            var response = await _httpClient.GetAsync($"{BaseApiUrl}/{postcode}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Returns up to <paramref name="count"/> restaurants in the given postcode.
        /// </summary>
        /// <param name="postcode">The UK postcode to search in.</param>
        /// <param name="count">The maximum number of restaurants to return.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="postcode"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is zero or negative.</exception>
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

        /// <summary>
        /// Returns up to <see cref="DefaultCount"/> restaurants in the given postcode.
        /// </summary>
        /// <param name="postcode">The UK postcode to search in.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="postcode"/> is null or whitespace.</exception>
        public Task<List<Restaurant>> GetRestaurantsAsync(string postcode)
            => GetRestaurantsAsync(postcode, DefaultCount);
    }
}