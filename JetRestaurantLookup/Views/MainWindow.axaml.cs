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

        var rawData = await _restaurantService.GetRawRestaurantsDataAsync("EC4M7RF");
        Debug.WriteLine(rawData);

        RestaurantsTextBlock.Text = rawData.Substring(0, 100);;

    }
}