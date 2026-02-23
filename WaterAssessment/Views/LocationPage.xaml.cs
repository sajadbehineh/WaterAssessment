using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class LocationPage : Page
{
    public AreaViewModel AreaViewModel { get; }
    public LocationViewModel LocationViewModel { get; }

    public LocationTypeViewModel LocationTypeViewModel { get; }

    public LocationPage()
    {
        this.InitializeComponent();
        this.LocationViewModel = App.Services.GetRequiredService<LocationViewModel>();
        this.AreaViewModel = App.Services.GetRequiredService<AreaViewModel>();
        this.LocationTypeViewModel = App.Services.GetRequiredService<LocationTypeViewModel>();
    }

    private void OnLocationDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Location item)
        {
            LocationViewModel.RequestDeleteLocationCommand.Execute(item);
        }
    }

    private void OnAreaDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Area item)
        {
            AreaViewModel.RequestDeleteAreaCommand.Execute(item);
        }
    }

    private void OnLocTypeDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is LocationType item)
        {
            LocationTypeViewModel.RequestDeleteLocationTypeCommand.Execute(item);
        }
    }
}
