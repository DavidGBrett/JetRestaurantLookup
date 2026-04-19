using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Mappers;

namespace JetRestaurantLookup.Tests.Mappers;

public class RatingMapperTests
{
    [Fact]
    public void ToModel_MapsAllFields()
    {
        var dto = new RatingDto
        {
            Count = 21,
            StarRating = 4.5
        };

        var model = RatingMapper.ToModel(dto);

        Assert.Equal(dto.Count, model.Count);
        Assert.Equal(dto.StarRating, model.StarRating);
    }
}
