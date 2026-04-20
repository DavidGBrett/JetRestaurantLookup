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
    public partial ObservableCollection<CuisineFilterViewModel> AvailableCuisines { get; set; } = [];

    private List<RestaurantCardViewModel> _allRestaurants = [];

    private void OnCuisineFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CuisineFilterViewModel.IsSelected))
            ApplyFilter();
    }

    private void ApplyFilter()
    {
        var selectedCuisines = AvailableCuisines.Where(c => c.IsSelected).Select(c => c.Name).ToList();

        if (selectedCuisines.Count == 0)
        {
            Restaurants = new ObservableCollection<RestaurantCardViewModel>(_allRestaurants);
            return;
        }

        Restaurants = new ObservableCollection<RestaurantCardViewModel>(
            _allRestaurants.Where(r => selectedCuisines.All(cuisine => r.Cuisines.Contains(cuisine))));
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

        AvailableCuisines = new ObservableCollection<CuisineFilterViewModel>(
            restaurants.SelectMany(r => r.Cuisines).Distinct().OrderBy(c => c)
                       .Select(name => new CuisineFilterViewModel { Name = name }));

        foreach (var cuisine in AvailableCuisines)
            cuisine.PropertyChanged += OnCuisineFilterChanged;

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
