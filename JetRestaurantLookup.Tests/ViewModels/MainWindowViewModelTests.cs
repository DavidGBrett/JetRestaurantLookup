using JetRestaurantLookup.Core.Models;
using JetRestaurantLookup.Core.Services;
using JetRestaurantLookup.ViewModels;

namespace JetRestaurantLookup.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static readonly string offerCategory1    = MainWindowViewModel._offerNames.ElementAt(0);
    private static readonly string offerCategory2    = MainWindowViewModel._offerNames.ElementAt(1);
    private static readonly string dietaryCategory1  = MainWindowViewModel._dietaryNames.ElementAt(0);
    private static readonly string dietaryCategory2  = MainWindowViewModel._dietaryNames.ElementAt(1);
    private static readonly string otherCategory1    = "Pizza";
    private static readonly string otherCategory2    = "Italian";
    private static readonly string otherCategory3    = "Burger";

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
            MakeRestaurant("1", otherCategory2),
            MakeRestaurant("2", otherCategory3)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Restaurants.Count);
    }

    [Fact]
    public async Task OneCategoryFilterSelected_ShowsOnlyMatchingRestaurants()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory2),
            MakeRestaurant("2", otherCategory3)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == otherCategory2).IsSelected = true;

        Assert.Single(vm.Restaurants);
        Assert.Equal("Restaurant 1", vm.Restaurants[0].Name);
    }

    [Fact]
    public async Task TwoCategoryFiltersSelected_AND_ShowsOnlyRestaurantsWithBoth()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory2, otherCategory1),
            MakeRestaurant("2", otherCategory2),
            MakeRestaurant("3", otherCategory1)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.First(c => c.Name == otherCategory2).IsSelected = true;
        vm.OtherCategories.First(c => c.Name == otherCategory1).IsSelected = true;

        Assert.Single(vm.Restaurants);
        Assert.Equal("Restaurant 1", vm.Restaurants[0].Name);
    }

    [Fact]
    public async Task DeselectingCategoryFilter_RestoresRestaurants()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory2),
            MakeRestaurant("2", otherCategory3)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        var categoryFilter = vm.OtherCategories.First(c => c.Name == otherCategory2);
        categoryFilter.IsSelected = true;
        Assert.Single(vm.Restaurants);

        categoryFilter.IsSelected = false;

        Assert.Equal(2, vm.Restaurants.Count);
    }

    [Fact]
    public async Task Load_SeparatesOfferCategoriesFromOtherCategories()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory1, offerCategory1),
            MakeRestaurant("2", otherCategory2, offerCategory2)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Contains(vm.OfferCategories, c => c.Name == offerCategory1);
        Assert.Contains(vm.OfferCategories, c => c.Name == offerCategory2);

        Assert.Contains(vm.OtherCategories, c => c.Name == otherCategory1);
        Assert.Contains(vm.OtherCategories, c => c.Name == otherCategory2);

        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == offerCategory1);
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == offerCategory2);

        Assert.DoesNotContain(vm.OfferCategories, c => c.Name == otherCategory1);
        Assert.DoesNotContain(vm.OfferCategories, c => c.Name == otherCategory2);
    }

    [Fact]
    public async Task Load_SeparatesDietaryCategoriesFromOtherCategories()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory1, dietaryCategory1),
            MakeRestaurant("2", otherCategory2, dietaryCategory2)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.Contains(vm.DietaryCategories, c => c.Name == dietaryCategory1 && c.Count == 1); // since dietary categories always show up
        Assert.Contains(vm.DietaryCategories, c => c.Name == dietaryCategory2 && c.Count == 1); // we check instead that the count is 1 not 0
        Assert.Contains(vm.OtherCategories, c => c.Name == otherCategory1);
        Assert.Contains(vm.OtherCategories, c => c.Name == otherCategory2);
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == dietaryCategory1);
        Assert.DoesNotContain(vm.OtherCategories, c => c.Name == dietaryCategory2);
        Assert.DoesNotContain(vm.DietaryCategories, c => c.Name == otherCategory1);
        Assert.DoesNotContain(vm.DietaryCategories, c => c.Name == otherCategory2);
    }

    [Fact]
    public async Task Load_DietaryCategories_AllAppearVisibleEvenWithNoMatches()
    {
        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", otherCategory1)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        Assert.All(MainWindowViewModel._dietaryNames, name =>
            Assert.Contains(vm.DietaryCategories, c => c.Name == name && c.IsVisible));
    }

    [Fact]
    public async Task NewSearch_RestoresSelectedFiltersIfStillPresent()
    {
        var categoryInBothResults = otherCategory1;
        var categoryInFirstResultOnly = otherCategory2;
        var categoryInSecondResultOnly = otherCategory3;

        // first results
        var restaurant1 = MakeRestaurant("1", categoryInBothResults, categoryInFirstResultOnly);
        var firstResults = new[] { restaurant1 };

        // second results
        var restaurant2 = MakeRestaurant("2", categoryInBothResults, categoryInSecondResultOnly);
        var restaurant3 = MakeRestaurant("3", categoryInSecondResultOnly);
        var secondResults = new[] { restaurant2, restaurant3};

        var vm = new MainWindowViewModel(new CallSequenceFakeRestaurantService(firstResults, secondResults));

        // load first results
        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        // we set both of these category filters in the first results
        vm.OtherCategories.Single(c => c.Name == categoryInBothResults).IsSelected = true;
        vm.OtherCategories.Single(c => c.Name == categoryInFirstResultOnly).IsSelected = true;

        // load second results
        vm.Postcode = "SW1A1AA";
        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        // Only the categoryInBothResults should still be selected
        Assert.True(vm.OtherCategories.Single(c => c.Name == categoryInBothResults).IsSelected);

        // Any others should not be selected
        Assert.True(vm.OtherCategories.All(c => c.Name == categoryInBothResults || !c.IsSelected));
        
        // Only restaurant 2 has categoryInBothResults so it should be the only one not filtered out
        var unfilteredRestaurant = Assert.Single(vm.Restaurants);
        Assert.Equal(restaurant2.Name, unfilteredRestaurant.Name);
    }

    [Fact]
    public async Task FilterApplied_NonDietaryCategoriesWithZeroMatchesAreHidden()
    {
        var matchingOfferCategory    = offerCategory1;
        var nonMatchingOfferCategory = offerCategory2;
        var selectedOtherCategory    = otherCategory1;
        var hiddenOtherCategory      = otherCategory3;

        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", selectedOtherCategory, matchingOfferCategory),
            MakeRestaurant("2", hiddenOtherCategory, nonMatchingOfferCategory)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);
        vm.OtherCategories.Single(c => c.Name == selectedOtherCategory).IsSelected = true;

        Assert.False(vm.OtherCategories.Single(c => c.Name == hiddenOtherCategory).IsVisible);
        Assert.False(vm.OfferCategories.Single(c => c.Name == nonMatchingOfferCategory).IsVisible);
        Assert.True(vm.OtherCategories.Single(c => c.Name == selectedOtherCategory).IsVisible);
        Assert.True(vm.OfferCategories.Single(c => c.Name == matchingOfferCategory).IsVisible);
    }

    [Fact]
    public async Task FilterApplied_DietaryCategories_AllAppearVisibleEvenWithNoMatches()
    {
        var selectedOtherCategory = otherCategory1;

        var vm = new MainWindowViewModel(new FakeRestaurantService(
            MakeRestaurant("1", selectedOtherCategory)));

        await vm.LoadRestaurantsCommand.ExecuteAsync(null);

        // apply filter, for restaurant with no dietary category
        vm.OtherCategories.Single(c => c.Name == selectedOtherCategory).IsSelected = true;

        // all dietary categories should still be visible
        Assert.All(MainWindowViewModel._dietaryNames, name =>
            Assert.Contains(vm.DietaryCategories, c => c.Name == name && c.IsVisible));
    }
}
