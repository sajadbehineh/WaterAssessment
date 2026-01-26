using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WaterAssessment.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ChannelsReportPage : Page
{
    public ChannelsReportViewModel ViewModel { get; }
    public ChannelsReportPage()
    {
        InitializeComponent();
        this.Name = "MyPage";
        ViewModel = new ChannelsReportViewModel();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // اگر لازم است هر بار که وارد صفحه می‌شوید اطلاعات رفرش شود:
        ViewModel.LoadData();
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
