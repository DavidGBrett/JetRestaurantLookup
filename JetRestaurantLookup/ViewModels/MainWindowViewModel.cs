using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetRestaurantLookup.Core.Services;
using JetRestaurantLookup.Core.Utilities;

namespace JetRestaurantLookup.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRestaurantService _restaurantService;

    public MainWindowViewModel(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [ObservableProperty]
    public partial string Postcode { get; set; } = "EC4M7RF";

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

    private static readonly HashSet<string> _offerNames = ["Deals", "Freebies", "Collect stamps"];
    private static readonly string[] _dietaryNames = ["Vegan", "Vegetarian", "Halal", "Gluten Free"];

    private List<RestaurantCardViewModel> _allRestaurants = [];


    private void OnCategoryFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CategoryFilterViewModel.IsSelected))
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

        var filtered = selected.Count == 0
            ? _allRestaurants
            : _allRestaurants.Where(r => selected.All(category => r.Cuisines.Contains(category))).ToList();

        Restaurants = new ObservableCollection<RestaurantCardViewModel>(filtered);

        var counts = filtered.SelectMany(r => r.Cuisines).GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
        foreach (var filter in GetAllFilters())
            filter.Count = counts.GetValueOrDefault(filter.Name);
    }

    [RelayCommand]
    private async Task LoadRestaurantsAsync()
    {
        if (string.IsNullOrWhiteSpace(Postcode))
        {
            StatusMessage = "Enter a postcode to search.";
            return;
        }

        var restaurants = await _restaurantService.GetRestaurantsAsync(Postcode);

        _allRestaurants = restaurants.Select(r => new RestaurantCardViewModel(r)).ToList();
        Restaurants = new ObservableCollection<RestaurantCardViewModel>(_allRestaurants);

        var categoryCounts = restaurants
            .SelectMany(r => r.Cuisines)
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());
        var allCategoryNames = categoryCounts.Keys.OrderBy(c => c).ToList();

        var previouslySelected = GetAllFilters().Where(c => c.IsSelected).Select(c => c.Name).ToHashSet();

        OfferCategories = CreateCategoryFilterGroup(
            allCategoryNames.Where(n => _offerNames.Contains(n)),
            categoryCounts,
            previouslySelected);

        DietaryCategories = CreateCategoryFilterGroup(
            _dietaryNames,
            categoryCounts,
            previouslySelected,
            alwaysVisible: true);

        OtherCategories = CreateCategoryFilterGroup(
            allCategoryNames.Where(n => !_offerNames.Contains(n) && !_dietaryNames.Contains(n)),
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
