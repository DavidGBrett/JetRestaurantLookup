using JetRestaurantLookup.Core.Models;

namespace JetRestaurantLookup.ViewModels;

public class RestaurantCardViewModel
{
    public string Name { get; }
    public string LogoUrl { get; }
    public string Address {get;}
    public List<string> Cuisines { get; }
    public string RatingSummary { get; }

    public RestaurantCardViewModel(Restaurant restaurant)
    {
        Name = restaurant.Name;
        LogoUrl = restaurant.LogoUrl;
        Cuisines = restaurant.Cuisines;
        RatingSummary = $"★ {restaurant.Rating.StarRating:F1} ({restaurant.Rating.Count} ratings)";
    
        var addressLine1 = restaurant.Address.FirstLine.Replace("\n", " ").Replace("\r", "").Trim();
        var addressLine2 = $"{restaurant.Address.City}, {restaurant.Address.PostalCode}";
        Address = $"{addressLine1}\n{addressLine2}";
    }
}
