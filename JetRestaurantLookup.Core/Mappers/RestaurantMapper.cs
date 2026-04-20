using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Mappers
{
    public static class RestaurantMapper
    {
        public static Restaurant ToModel(RestaurantDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            Address = AddressMapper.ToModel(dto.Address),
            Rating = RatingMapper.ToModel(dto.Rating),
            Cuisines = dto.Cuisines.Select(c => c.Name).ToList()
        };
    }
}
