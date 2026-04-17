using System.Net.Http;
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

        public async Task<string> GetRawRestaurantsDataAsync(string postcode)
        {
            var response = await _httpClient.GetAsync($"{BaseApiUrl}/{postcode}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

    }
}