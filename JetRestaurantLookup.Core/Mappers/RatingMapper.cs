using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Mappers
{
    public static class RatingMapper
    {
        public static Rating ToModel(RatingDto dto) => new()
        {
            Count = dto.Count,
            StarRating = dto.StarRating
        };
    }
}
