using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JetRestaurantLookup.ViewModels;

public partial class StarFilterViewModel : ObservableObject
{
    private readonly Action<int> _onSelected;

    public StarFilterViewModel(int value, Action<int> onSelected)
    {
        Value = value;
        _onSelected = onSelected;
    }

    public int Value { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [RelayCommand]
    private void Select()
    {
        _onSelected(Value);
    }
}
