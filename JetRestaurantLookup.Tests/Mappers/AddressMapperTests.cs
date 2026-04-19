using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Mappers;

namespace JetRestaurantLookup.Tests.Mappers;

public class AddressMapperTests
{
    [Fact]
    public void ToModel_MapsAllFields()
    {
        var dto = new AddressDto
        {
            City = "London",
            FirstLine = "1 Example Street",
            PostalCode = "EC1M 6HR"
        };

        var model = AddressMapper.ToModel(dto);

        Assert.Equal(dto.City, model.City);
        Assert.Equal(dto.FirstLine, model.FirstLine);
        Assert.Equal(dto.PostalCode, model.PostalCode);
    }
}
