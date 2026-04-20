using System.Net;
using System.Net.Http;
using JetRestaurantLookup.Core.Services;
using RichardSzalay.MockHttp;

namespace JetRestaurantLookup.Tests.Services;

public class RestaurantServiceTests
{
    private const string TestPostcode = "EC4M7RF";

    private static (RestaurantService Service, MockHttpMessageHandler MockHttp) CreateService()
    {
        var mockHttp = new MockHttpMessageHandler();
        var service = new RestaurantService(mockHttp.ToHttpClient());
        return (service, mockHttp);
    }

    private static string BuildRestaurantJson(int i) =>
        $$"""
        {
            "id": "{{i}}",
            "name": "Restaurant {{i}}",
            "logoUrl": "https://d30v2pzvrfyzpo.cloudfront.net/uk/images/restaurants/212716.gif",
            "address": { "city": "London", "firstLine": "{{i}} Example Street", "postalCode": "EC4M 7RF" },
            "rating": { "count": 100, "starRating": 4.5 },
            "cuisines": [{ "name": "Italian" }]
        }
        """;

    private static string BuildResponseJson(int restaurantCount)
    {
        var restaurants = string.Join(",", Enumerable.Range(1, restaurantCount).Select(BuildRestaurantJson));
        return $$"""{ "restaurants": [{{restaurants}}] }""";
    }

    // --- Happy path ---

    [Fact]
    public async Task GetRestaurantsAsync_ValidPostcode_ParsesResponseIntoRestaurant()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", BuildResponseJson(1));

        var results = await service.GetRestaurantsAsync(TestPostcode);

        var restaurant = Assert.Single(results);
        Assert.Equal("1", restaurant.Id);
    }

    [Fact]
    public async Task GetRestaurantsAsync_CountRespected_ReturnsTrimmedList()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", BuildResponseJson(5));

        var results = await service.GetRestaurantsAsync(TestPostcode, count: 3);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetRestaurantsAsync_CountExceedsAvailable_ReturnsAllRestaurants()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", BuildResponseJson(2));

        var results = await service.GetRestaurantsAsync(TestPostcode, count: 10);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetRestaurantsAsync_EmptyRestaurantsArray_ReturnsEmptyList()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", """{ "restaurants": [] }""");

        var results = await service.GetRestaurantsAsync(TestPostcode);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetRestaurantsAsync_DefaultCount_ReturnsUpToTen()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", BuildResponseJson(15));

        var results = await service.GetRestaurantsAsync(TestPostcode);

        Assert.Equal(10, results.Count);
    }

    // --- Input validation ---

    [Fact]
    public async Task GetRestaurantsAsync_NullPostcode_ThrowsArgumentNullException()
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.GetRestaurantsAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetRestaurantsAsync_WhitespacePostcode_ThrowsArgumentException(string postcode)
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetRestaurantsAsync(postcode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetRestaurantsAsync_InvalidCount_ThrowsArgumentOutOfRangeException(int count)
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetRestaurantsAsync(TestPostcode, count));
    }

    // --- HTTP failures ---

    [Fact]
    public async Task GetRestaurantsAsync_ApiReturns404_ThrowsHttpRequestException()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond(HttpStatusCode.NotFound);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetRestaurantsAsync(TestPostcode));
    }

    [Fact]
    public async Task GetRestaurantsAsync_ApiReturns500_ThrowsHttpRequestException()
    {
        var (service, mockHttp) = CreateService();
        mockHttp.When($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetRestaurantsAsync(TestPostcode));
    }

    // --- Postcode formatting ---

    [Theory]
    [InlineData("EC4M 7RF")]
    [InlineData("ec4m7rf")]
    [InlineData("ec4m 7rf")]
    public async Task GetRestaurantsAsync_PostcodeVariants_UsesCorrectApiFormat(string postcode)
    {
        var (service, mockHttp) = CreateService();
        mockHttp.Expect($"{RestaurantService.BaseApiUrl}/{TestPostcode}")
                .Respond("application/json", BuildResponseJson(1));

        await service.GetRestaurantsAsync(postcode);

        mockHttp.VerifyNoOutstandingExpectation();
    }
}
