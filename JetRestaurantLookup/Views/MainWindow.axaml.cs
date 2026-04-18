using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JetRestaurantLookup.Core.Services;

namespace JetRestaurantLookup.Views;

public partial class MainWindow : Window
{
    private readonly RestaurantService _restaurantService = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        RestaurantsTextBlock.Text = "Loading...";

        var restaurants = await _restaurantService.GetRestaurantsAsync("EC4M7RF");
        Debug.WriteLine(restaurants);

        RestaurantsTextBlock.Text = string.Join(",\n",restaurants);

    }
}