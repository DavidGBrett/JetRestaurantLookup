using JetRestaurantLookup.Core.Models;
using JetRestaurantLookup.Core.Services;
using JetRestaurantLookup.ViewModels;

namespace JetRestaurantLookup.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static Restaurant MakeRestaurant(string id, params string[] cuisines) => new()
    {
        Id = id,
        Name = $"Restaurant {id}",
        LogoUrl = "https://example.com/logo.gif",
        Address = new Address { City = "London", FirstLine = "1 Example Street", PostalCode = "EC1A 1AA" },
        Rating = new Rating { Count = 10, StarRating = 4.0 },
        Cuisines = cuisines.ToList()
    };

    private sealed class FakeRestaurantService(params Restaurant[] restaurants) : IRestaurantService
    {
        public Task<List<Restaurant>> GetRestaurantsAsync(string postcode, int count = 10)
            => Task.FromResult(restaurants.Take(count).ToList());
    }

    [Fact]
    public async Task NoFiltersSelected_ShowsAllRestaurants()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Restaurants.Count);
    }

    [Fact]
    public async Task OneCategoryFilterSelected_ShowsOnlyMatchingRestaurants()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == "Italian").IsSelected = true;

        Assert.Single(vm.Restaurants);
        Assert.Equal("Restaurant 1", vm.Restaurants[0].Name);
    }

    [Fact]
    public async Task TwoCategoryFiltersSelected_AND_ShowsOnlyRestaurantsWithBoth()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian", "Pizza"),
            MakeRestaurant("2", "Italian"),
            MakeRestaurant("3", "Pizza")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == "Italian").IsSelected = true;
        vm.OtherCategories.First(c => c.Name == "Pizza").IsSelected = true;

        Assert.Single(vm.Restaurants);
        Assert.Equal("Restaurant 1", vm.Restaurants[0].Name);
    }

    [Fact]
    public async Task DeselectingCategoryFilter_RestoresRestaurants()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        var italianFilter = vm.OtherCategories.First(c => c.Name == "Italian");
        italianFilter.IsSelected = true;
        Assert.Single(vm.Restaurants);

        italianFilter.IsSelected = false;

        Assert.Equal(2, vm.Restaurants.Count);
    }

    [Fact]
    public async Task NewSearch_ResetsFiltersAndShowsNewCategories()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == "Italian").IsSelected = true;
        Assert.Single(vm.Restaurants);

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Restaurants.Count);
        Assert.All(vm.OtherCategories, c => Assert.False(c.IsSelected));
    }

    [Fact]
    public async Task Load_SeparatesOfferCategoriesFromOtherCategories()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Pizza", "Deals"),
            MakeRestaurant("2", "Italian", "Freebies")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Contains(vm.OfferCategories, c => c.Name == "Deals");
        Assert.Contains(vm.OfferCategories, c => c.Name == "Freebies");
        Assert.Contains(vm.OtherCategories, c => c.Name == "Pizza");
        Assert.Contains(vm.OtherCategories, c => c.Name == "Italian");
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == "Deals");
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == "Freebies");
        Assert.DoesNotContain(vm.OfferCategories, c => c.Name == "Pizza");
        Assert.DoesNotContain(vm.OfferCategories, c => c.Name == "Italian");
    }
}
