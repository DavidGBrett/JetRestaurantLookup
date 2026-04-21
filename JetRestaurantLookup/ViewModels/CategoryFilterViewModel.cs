using CommunityToolkit.Mvvm.ComponentModel;

namespace JetRestaurantLookup.ViewModels;

public partial class CategoryFilterViewModel : ObservableObject
{
    public required string Name { get; init; }
    public bool AlwaysVisible { get; init; }

    [ObservableProperty]
    public partial int Count { get; set; }

    public string DisplayName => $"{Name} ({Count})";
    public bool IsVisible => Count > 0 || AlwaysVisible;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    partial void OnCountChanged(int value)
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(IsVisible));
    }
}
