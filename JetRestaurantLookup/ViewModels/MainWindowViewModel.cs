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
    private static readonly HashSet<string> _dietaryNames = ["Vegan", "Vegetarian", "Halal", "Gluten Free"];

    private List<RestaurantCardViewModel> _allRestaurants = [];

    private void OnCategoryFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CategoryFilterViewModel.IsSelected))
            ApplyFilter();
    }

    private void ApplyFilter()
    {
        var selected = OfferCategories.Concat(DietaryCategories).Concat(OtherCategories)
            .Where(c => c.IsSelected).Select(c => c.Name).ToList();

        if (selected.Count == 0)
        {
            Restaurants = new ObservableCollection<RestaurantCardViewModel>(_allRestaurants);
            return;
        }

        Restaurants = new ObservableCollection<RestaurantCardViewModel>(
            _allRestaurants.Where(r => selected.All(category => r.Cuisines.Contains(category))));
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

        var allCategoryNames = restaurants.SelectMany(r => r.Cuisines).Distinct().OrderBy(c => c).ToList();

        OfferCategories = new ObservableCollection<CategoryFilterViewModel>(
            allCategoryNames.Where(n => _offerNames.Contains(n))
                           .Select(name => new CategoryFilterViewModel { Name = name }));

        DietaryCategories = new ObservableCollection<CategoryFilterViewModel>(
            allCategoryNames.Where(n => _dietaryNames.Contains(n))
                           .Select(name => new CategoryFilterViewModel { Name = name }));

        OtherCategories = new ObservableCollection<CategoryFilterViewModel>(
            allCategoryNames.Where(n => !_offerNames.Contains(n) && !_dietaryNames.Contains(n))
                           .Select(name => new CategoryFilterViewModel { Name = name }));

        foreach (var filter in OfferCategories.Concat(DietaryCategories).Concat(OtherCategories))
            filter.PropertyChanged += OnCategoryFilterChanged;

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
