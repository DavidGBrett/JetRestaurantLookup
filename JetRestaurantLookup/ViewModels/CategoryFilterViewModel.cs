using CommunityToolkit.Mvvm.ComponentModel;

namespace JetRestaurantLookup.ViewModels;

public partial class CategoryFilterViewModel : ObservableObject
{
    public required string Name { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
