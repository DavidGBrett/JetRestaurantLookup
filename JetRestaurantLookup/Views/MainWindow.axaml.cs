using Avalonia.Controls;
using Avalonia.Interactivity;
using JetRestaurantLookup.ViewModels;

namespace JetRestaurantLookup.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.LoadRestaurantsCommand.Execute(null);
    }
}