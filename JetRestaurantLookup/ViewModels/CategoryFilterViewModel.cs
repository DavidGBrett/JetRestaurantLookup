using CommunityToolkit.Mvvm.ComponentModel;

namespace JetRestaurantLookup.ViewModels;

public partial class CategoryFilterViewModel : ObservableObject
{
    public required string Name { get; init; }
    public int Count { get; init; }
    public string DisplayName => $"{Name} ({Count})";

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
