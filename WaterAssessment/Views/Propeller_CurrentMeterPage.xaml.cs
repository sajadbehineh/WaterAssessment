using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class Propeller_CurrentMeterPage : Page
{
    public PropellerViewModel PropellerViewModel { get; }
    public CurrentMeterViewModel CurrentMeterViewModel { get; }

    public Propeller_CurrentMeterPage()
    {
        this.InitializeComponent();
        DataContext = this;
        CurrentMeterViewModel = App.Services.GetRequiredService<CurrentMeterViewModel>();
        PropellerViewModel = App.Services.GetRequiredService<PropellerViewModel>();
    }

    private void OnPropellerDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Propeller item)
        {
            PropellerViewModel.RequestDeletePropellerCommand.Execute(item);
        }
    }

    private void OnCurrentMeterDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is CurrentMeter item)
        {
            CurrentMeterViewModel.RequestDeleteCurrentMeterCommand.Execute(item);
        }
    }
}