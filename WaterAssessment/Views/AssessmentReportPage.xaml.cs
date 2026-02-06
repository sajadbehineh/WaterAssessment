using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;

namespace WaterAssessment.Views;

public sealed partial class AssessmentReportPage : Page
{
    public AssessmentReportViewModel ViewModel { get; }
    public AssessmentReportPage()
    {
        InitializeComponent();
        this.Name = "MyPage";
        ViewModel = App.Services.GetRequiredService<AssessmentReportViewModel>();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _ = ViewModel.LoadDataAsync();
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        // 1. دکمه‌ای که کلیک شده را می‌گیریم
        if (sender is Button btn && btn.DataContext is Assessment item)
        {
            // 2. متد ویومدل را صدا می‌زنیم
            ViewModel.EditAssessmentCommand.Execute(item);
        }
    }

    // متد کلیک برای حذف
    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Assessment item)
        {
            // چون متد حذفсинک (Async) است، مستقیم آن را صدا می‌زنیم (نه از طریق Command)
            // یا اگر کامند AsyncRelayCommand است:
            await ViewModel.DeleteAssessmentCommand.ExecuteAsync(item);
        }
    }
}
