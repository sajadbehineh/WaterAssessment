using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Windowing;
using System.Diagnostics.Metrics;
using WaterAssessment.Messages;
using WaterAssessment.Models;
using WaterAssessment.Views;

namespace WaterAssessment;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new MainViewModel();

    internal static MainWindow Instance { get; private set; }

    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        this.RootGrid.DataContext = this;
        this.SetWindowSize(1650, 800);
        this.AppWindow.SetPresenter(AppWindowPresenterKind.Default);

        // گوش دادن به پیام لاگین برای نویگیشن
        WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (r, m) =>
        {
            // وقتی پیام لاگین رسید، به صفحه اصلی بروید
            // چون روی ترد UI هستیم مستقیم نویگیت میکنیم
            ShellPage.Instance.Navigate(typeof(ChannelsReportPage)); // یا هر صفحه پیش‌فرض دیگر
        });
        //this.AppWindow.DispatcherQueue.TryEnqueue(() =>
        //{
        //    if (!ViewModel.IsLoggedIn)
        //    {
        //        ShellPage.Instance.Navigate(typeof(LoginPage));
        //    }
        //});
        // رویداد لود شدن پنجره برای نمایش صفحه لاگین
        //this.Activated += MainWindow_Activated;
        //ShellPage.Instance.Loaded += Instance_Loaded;
        //ShellPage.Instance.Navigate(typeof(LoginPage));
    }

    //private void Instance_Loaded(object sender, RoutedEventArgs e)
    //{
    //    ShellPage.Instance.Loaded -= Instance_Loaded;
    //    // هدایت اولیه به صفحه لاگین
    //    if (!ViewModel.IsLoggedIn)
    //    {
    //        ShellPage.Instance.Navigate(typeof(LoginPage));
    //    }
    //}

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // فقط بار اول اجرا شود
        this.Activated -= MainWindow_Activated;

        // هدایت اولیه به صفحه لاگین
        if (!ViewModel.IsLoggedIn)
        {
            ShellPage.Instance.Navigate(typeof(LoginPage));
        }
    }

    private async void NavigationViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var menuItem = sender as NavigationViewItem;

        if (menuItem?.Tag is not null)
        {
            switch (menuItem.Tag)
            {
                case "AssessmentForm":
                    ShellPage.Instance.Navigate(typeof(AssessmentFormPage));
                    break;

                case "InsertEmployees":
                    ShellPage.Instance.Navigate(typeof(EmployeePage));
                    break;

                case "Location":
                    ShellPage.Instance.Navigate(typeof(LocationPage));
                    break;

                case "Propeller_CurrentMeter":
                    ShellPage.Instance.Navigate(typeof(Propeller_CurrentMeterPage));
                    break;

                case "Settings":
                    ShellPage.Instance.Navigate(typeof(SettingsPage));
                    break;
                case "ChannelsReportPage":
                    ShellPage.Instance.Navigate(typeof(ChannelsReportPage));
                    break;
            }
        }
    }
}