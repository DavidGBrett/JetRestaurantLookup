using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.ViewModels;

public class RestaurantCardViewModel
{
    public string Name { get; }
    public string LogoUrl { get; }
    public string Address { get; }
    public List<string> Cuisines { get; }
    public string RatingSummary { get; }
    public IRelayCommand OpenGoogleMapsCommand { get; }
    public IRelayCommand OpenJustEatCommand { get; }

    public RestaurantCardViewModel(Restaurant restaurant)
    {
        Name = restaurant.Name;
        LogoUrl = restaurant.LogoUrl;
        Cuisines = restaurant.Cuisines;
        RatingSummary = $"★ {restaurant.Rating.StarRating:F1} ({restaurant.Rating.Count} ratings)";

        var addressLine1 = restaurant.Address.FirstLine.Replace("\n", " ").Replace("\r", "").Trim();
        var addressLine2 = $"{restaurant.Address.City}, {restaurant.Address.PostalCode}";
        Address = $"{addressLine1}\n{addressLine2}";

        var mapsQuery = Uri.EscapeDataString($"{Name} {addressLine1} {addressLine2}");
        OpenGoogleMapsCommand = new RelayCommand(() =>
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://www.google.com/maps/search/?api=1&query={mapsQuery}",
                UseShellExecute = true
            }));

        var uniqueName = restaurant.UniqueName;
        OpenJustEatCommand = new RelayCommand(() =>
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://www.just-eat.co.uk/restaurants-{uniqueName}/menu",
                UseShellExecute = true
            }));
    }
}
