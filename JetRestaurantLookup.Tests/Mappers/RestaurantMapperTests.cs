using JetRestaurantLookup.Core.Dtos;
using JetRestaurantLookup.Core.Mappers;

namespace JetRestaurantLookup.Tests.Mappers;

public class RestaurantMapperTests
{
    private static RestaurantDto BuildDto(List<CuisineDto>? cuisines = null) => new()
    {
        Id = "123",
        Name = "Test Restaurant",
        LogoUrl = "https://d30v2pzvrfyzpo.cloudfront.net/uk/images/restaurants/212716.gif",
        Address = new AddressDto
        {
            City = "London",
            FirstLine = "1 Example Street",
            PostalCode = "EC1M 6HR"
        },
        Rating = new RatingDto
        {
            Count = 100,
            StarRating = 4.2
        },
        Cuisines = cuisines ?? [new CuisineDto { Name = "Italian" }]
    };

    [Fact]
    public void ToModel_MapsScalarFields()
    {
        var dto = BuildDto();

        var model = RestaurantMapper.ToModel(dto);

        Assert.Equal(dto.Id, model.Id);
        Assert.Equal(dto.Name, model.Name);
        Assert.Equal(dto.LogoUrl, model.LogoUrl);
    }

    [Fact]
    public void ToModel_MapsNestedAddress()
    {
        var dto = BuildDto();

        var model = RestaurantMapper.ToModel(dto);

        Assert.Equal(dto.Address.City, model.Address.City);
        Assert.Equal(dto.Address.FirstLine, model.Address.FirstLine);
        Assert.Equal(dto.Address.PostalCode, model.Address.PostalCode);
    }

    [Fact]
    public void ToModel_MapsNestedRating()
    {
        var dto = BuildDto();

        var model = RestaurantMapper.ToModel(dto);

        Assert.Equal(dto.Rating.Count, model.Rating.Count);
        Assert.Equal(dto.Rating.StarRating, model.Rating.StarRating);
    }

    [Fact]
    public void ToModel_FlattensCuisineNamesToStrings()
    {
        var dto = BuildDto([
            new CuisineDto { Name = "Thai" },
            new CuisineDto { Name = "Asian" }
        ]);

        var model = RestaurantMapper.ToModel(dto);

        Assert.Equal(["Thai", "Asian"], model.Cuisines);
    }

    [Fact]
    public void ToModel_WithNoCuisines_ReturnsEmptyList()
    {
        var dto = BuildDto([]);

        var model = RestaurantMapper.ToModel(dto);

        Assert.Empty(model.Cuisines);
    }
}
