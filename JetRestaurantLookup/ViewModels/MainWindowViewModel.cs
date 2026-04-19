using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetRestaurantLookup.Core.Models;
using JetRestaurantLookup.Core.Services;

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
    public partial ObservableCollection<Restaurant> Restaurants { get; set; } = [];

    [RelayCommand]
    private async Task LoadRestaurantsAsync()
    {
        var restaurants = await _restaurantService.GetRestaurantsAsync(Postcode);

        Restaurants = new ObservableCollection<Restaurant>(restaurants);
    }
}
