using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.Core.Mappers
{
    public static class AddressMapper
    {
        public static Address ToModel(AddressDto dto) => new()
        {
            City = dto.City,
            FirstLine = dto.FirstLine,
            PostalCode = dto.PostalCode
        };
    }
}
