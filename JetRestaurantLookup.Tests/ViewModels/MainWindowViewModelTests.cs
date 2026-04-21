using JetRestaurantLookup.Core.Models;
using JetRestaurantLookup.Core.Services;
using JetRestaurantLookup.ViewModels;

namespace JetRestaurantLookup.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static Restaurant MakeRestaurant(string id, params string[] cuisines) => new()
    {
        Id = id,
        UniqueName = $"restaurant-{id}",
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

    private sealed class CallSequenceFakeRestaurantService(
        IEnumerable<Restaurant> firstCall,
        IEnumerable<Restaurant> secondCall) : IRestaurantService
    {
        private int _callCount;

        public Task<List<Restaurant>> GetRestaurantsAsync(string postcode, int count = 10)
        {
            _callCount++;
            return Task.FromResult((_callCount == 1 ? firstCall : secondCall).Take(count).ToList());
        }
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
    public async Task NewSearch_PersistsSelectedFiltersWhenCategoryStillPresent()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Italian"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == "Italian").IsSelected = true;
        Assert.Single(vm.Restaurants);

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.True(vm.OtherCategories.First(c => c.Name == "Italian").IsSelected);
        Assert.Single(vm.Restaurants); // filter still active
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

    [Fact]
    public async Task Load_SeparatesDietaryCategoriesFromOtherCategories()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Pizza", "Vegan"),
            MakeRestaurant("2", "Italian", "Halal")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Contains(vm.DietaryCategories, c => c.Name == "Vegan" && c.Count == 1); // since dietary categories always show up
        Assert.Contains(vm.DietaryCategories, c => c.Name == "Halal" && c.Count == 1); // we check instead that the count is 1 not 0
        Assert.Contains(vm.OtherCategories, c => c.Name == "Pizza");
        Assert.Contains(vm.OtherCategories, c => c.Name == "Italian");
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == "Vegan");
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == "Halal");
        Assert.DoesNotContain(vm.DietaryCategories, c => c.Name == "Pizza");
        Assert.DoesNotContain(vm.DietaryCategories, c => c.Name == "Italian");
    }

    [Fact]
    public async Task Load_DietaryCategories_AllAppearVisibleEvenWithNoMatches()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Pizza")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Contains(vm.DietaryCategories, c => c.Name == "Vegan"       && c.IsVisible);
        Assert.Contains(vm.DietaryCategories, c => c.Name == "Vegetarian"  && c.IsVisible);
        Assert.Contains(vm.DietaryCategories, c => c.Name == "Halal"       && c.IsVisible);
        Assert.Contains(vm.DietaryCategories, c => c.Name == "Gluten Free" && c.IsVisible);
    }

    [Fact]
    public async Task NewSearch_RestoresSelectedFiltersIfStillPresent()
    {
        var firstResults = new[] { MakeRestaurant("1", "Pizza", "Vegan") };
        var secondResults = new[] { MakeRestaurant("2", "Burger", "Vegan") };
        var vm = new MainWindowViewModel(new CallSequenceFakeRestaurantService(firstResults, secondResults));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.Single(c => c.Name == "Pizza").IsSelected = true;
        vm.DietaryCategories.Single(c => c.Name == "Vegan").IsSelected = true;

        vm.Postcode = "SW1A1AA";
        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.True(vm.DietaryCategories.Single(c => c.Name == "Vegan").IsSelected);
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == "Pizza"); // not in new results
        // Only "Vegan" filter still selected; "Burger" restaurant has Vegan so it shows
        Assert.Contains(vm.Restaurants, r => r.Name == "Restaurant 2");
    }

    [Fact]
    public async Task FilterApplied_NonDietaryCategoriesWithZeroMatchesAreHidden()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Pizza", "Deals"),
            MakeRestaurant("2", "Chinese", "Freebies")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.Single(c => c.Name == "Pizza").IsSelected = true;

        Assert.False(vm.OtherCategories.Single(c => c.Name == "Chinese").IsVisible);
        Assert.False(vm.OfferCategories.Single(c => c.Name == "Freebies").IsVisible);
        Assert.True(vm.OtherCategories.Single(c => c.Name == "Pizza").IsVisible);
        Assert.True(vm.OfferCategories.Single(c => c.Name == "Deals").IsVisible);
    }

    [Fact]
    public async Task FilterApplied_DietaryCategoryWithZeroMatchesRemainsVisible()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", "Pizza", "Vegan"),
            MakeRestaurant("2", "Chinese")));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.Single(c => c.Name == "Pizza").IsSelected = true;

        Assert.True(vm.DietaryCategories.Single(c => c.Name == "Halal").IsVisible);
    }
}
