using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Services
{
    public interface IRestaurantService
    {
        /// <summary>
        /// Returns up to <paramref name="count"/> restaurants in the given postcode.
        /// </summary>
        /// <param name="postcode">The UK postcode to search in.</param>
        /// <param name="count">The maximum number of restaurants to return.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="postcode"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is zero or negative.</exception>
        Task<List<Restaurant>> GetRestaurantsAsync(string postcode, int count);

        /// <summary>
        /// Returns up to <see cref="DefaultCount"/> restaurants in the given postcode.
        /// </summary>
        /// <param name="postcode">The UK postcode to search in.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="postcode"/> is null or whitespace.</exception>
        Task<List<Restaurant>> GetRestaurantsAsync(string postcode);
    }
}
