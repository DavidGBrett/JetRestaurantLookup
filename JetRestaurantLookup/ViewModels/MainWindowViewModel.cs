using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetRestaurantLookup.Core.Models;
using JetRestaurantLookup.Core.Services;
using JetRestaurantLookup.Core.Utilities;

namespace JetRestaurantLookup.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRestaurantService _restaurantService;

    public MainWindowViewModel(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;

        RatingStars = new ObservableCollection<StarFilterViewModel>(
            Enumerable.Range(1, 5)
            .Select(value => new StarFilterViewModel(value, ToggleMinimumRating))
        );

        UpdateRatingStarSelection();
    }
    public ObservableCollection<StarFilterViewModel> RatingStars { get; }

    private int _minimumRating = 0;

    public int? MinimumRating
    {
        get => _minimumRating;
        set
        {
            var parsedValue = value ?? 0;

            // Only accept 0-5
            if (parsedValue >= 0 && parsedValue <= 5)
            {
                if (parsedValue != _minimumRating)
                {
                    _minimumRating = parsedValue;
                    OnPropertyChanged(nameof(MinimumRating));
                    UpdateRatingStarSelection();
                    ApplyFilter();
                }
            }
        }
    }

    private void UpdateRatingStarSelection()
    {
        foreach (var ratingStar in RatingStars)
            ratingStar.IsSelected = ratingStar.Value <= _minimumRating;
    }

    [RelayCommand]
    private void ToggleMinimumRating(int rating)
    {
        // clear rating if the current minimum rating star is clicked again
        if (rating == _minimumRating){
            MinimumRating = 0;
        }
        else{
            MinimumRating = rating;
        }
    }

    [ObservableProperty]
    public partial string Postcode { get; set; } = "EC4M 7RF";

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<RestaurantCardViewModel> Restaurants { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategoryFilterViewModel> OfferCategories { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategoryFilterViewModel> DietaryCategories { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategoryFilterViewModel> OtherCategories { get; set; } = [];

    internal static readonly FrozenSet<string> _offerNames = ["Deals", "Freebies", "Collect stamps", "Cheeky Tuesday"];
    internal static readonly FrozenSet<string> _dietaryNames = ["Vegan", "Vegetarian", "Halal", "Gluten Free"];

    private List<RestaurantCardViewModel> _allRestaurants = [];


    private void OnCategoryFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CategoryFilterViewModel.IsSelected))
            ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private IEnumerable<CategoryFilterViewModel> GetAllFilters() => OfferCategories.Concat(DietaryCategories).Concat(OtherCategories);

    private static ObservableCollection<CategoryFilterViewModel> CreateCategoryFilterGroup(
        IEnumerable<string> names,
        Dictionary<string, int> counts,
        HashSet<string> selected,
        bool alwaysVisible = false)
    {
        return new ObservableCollection<CategoryFilterViewModel>(
            names.Select(name => new CategoryFilterViewModel
            {
                Name = name,
                AlwaysVisible = alwaysVisible,
                Count = counts.GetValueOrDefault(name),
                IsSelected = selected.Contains(name)
            }));
    }

    private void ApplyFilter()
    {
        var selected = GetAllFilters().Where(c => c.IsSelected).Select(c => c.Name).ToList();
        var searchTerms = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var withCategory = selected.Count == 0 && searchTerms.Length == 0
            ? _allRestaurants
            : _allRestaurants.Where(r => selected.All(category => r.Cuisines.Contains(category))).ToList();

        var withSearchTerm = searchTerms.Length == 0
            ? withCategory
            : withCategory.Where(r => searchTerms.All(
                term => r.Name.Contains(term, StringComparison.OrdinalIgnoreCase) 
                || 
                r.Cuisines.Any(cuisine => cuisine.Contains(term, StringComparison.OrdinalIgnoreCase))
            ));

        var withMinimumRating = withSearchTerm.Where(r => r.StarRating >= _minimumRating);

        var filtered = withMinimumRating;

        var ordered = filtered.OrderByDescending(r => r.StarRating);

        Restaurants = new ObservableCollection<RestaurantCardViewModel>(ordered);

        var counts = filtered.SelectMany(r => r.Cuisines).GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
        foreach (var filter in GetAllFilters())
            filter.Count = counts.GetValueOrDefault(filter.Name);
    }

    [RelayCommand]
    private async Task LoadRestaurantsAsync()
    {
        var previouslySelected = GetAllFilters().Where(c => c.IsSelected).Select(c => c.Name).ToHashSet();
        
        Restaurants = [];

        if (string.IsNullOrWhiteSpace(Postcode))
        {
            StatusMessage = "Enter a postcode to search.";
            return;
        }

        StatusMessage = "Loading...";

        List<Restaurant> restaurants;
        try
        {
            restaurants = await _restaurantService.GetRestaurantsAsync(Postcode);
        }
        catch (HttpRequestException)
        {
            StatusMessage = "We couldn't load the restaurants right now. Check your internet connection and try again.";
            return;
        }
        catch (Exception)
        {
            StatusMessage = "Something went wrong while loading the restaurants.";
            return;
        }

        _allRestaurants = restaurants.Select(r => new RestaurantCardViewModel(r)).ToList();
        Restaurants = new ObservableCollection<RestaurantCardViewModel>(_allRestaurants);

        var categoryCounts = restaurants
            .SelectMany(r => r.Cuisines)
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());
        var allCategoryNames = categoryCounts.Keys.OrderBy(c => c).ToList();

        var availableOfferCategoryNames = new List<string>();
        var availableOtherCategoryNames = new List<string>();

        foreach (var name in allCategoryNames)
        {
            if (_offerNames.Contains(name))
                availableOfferCategoryNames.Add(name);
            else if (!_dietaryNames.Contains(name))
                availableOtherCategoryNames.Add(name);
        }

        OfferCategories = CreateCategoryFilterGroup(
            availableOfferCategoryNames,
            categoryCounts,
            previouslySelected);

        DietaryCategories = CreateCategoryFilterGroup(
            _dietaryNames,
            categoryCounts,
            previouslySelected,
            alwaysVisible: true);

        OtherCategories = CreateCategoryFilterGroup(
            availableOtherCategoryNames,
            categoryCounts,
            previouslySelected);

        foreach (var filter in GetAllFilters())
            filter.PropertyChanged += OnCategoryFilterChanged;

        ApplyFilter();

        if (restaurants.Count == 0)
        {
            StatusMessage = Postcodes.IsValid(Postcode)
                ? "No restaurants found for this postcode."
                : "No restaurants found. Your postcode doesn't look right — check it and try again.";
        }
        else
        {
            StatusMessage = null;
        }
    }
}
