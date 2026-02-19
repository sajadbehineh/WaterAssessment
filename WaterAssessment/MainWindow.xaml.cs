using System.IO;
using Windows.UI;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using WaterAssessment.Messages;
using WaterAssessment.Services;
using WaterAssessment.Views;
using static WaterAssessment.ViewModel.MainViewModel;

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
        //this.SetWindowSize(1650, 800);
        this.AppWindow.SetPresenter(AppWindowPresenterKind.Default);
        ConfigureWindowIdentity();
        ApplyTitleBarColors();
        RootGrid.ActualThemeChanged += (_, _) => ApplyTitleBarColors();
        Activated += (_, _) => ApplyTitleBarColors();

        // گوش دادن به پیام لاگین برای نویگیشن
        WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (r, m) =>
        {
            // وقتی پیام لاگین رسید، به صفحه اصلی بروید
            // چون روی ترد UI هستیم مستقیم نویگیت میکنیم
            ShellPage.Instance.Navigate(typeof(AssessmentReportPage)); // یا هر صفحه پیش‌فرض دیگر
        });
        // مدیریت خروج از حساب
        WeakReferenceMessenger.Default.Register<LogoutMessage>(this, (r, m) =>
        {
            MyShell.Navigate(typeof(LoginPage));
        });
        ShellPage.Instance.Loaded += Instance_Loaded;
    }

    private void ConfigureWindowIdentity()
    {
        Title = "مطالعات و آبشناسی";

        var iconPathCandidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "dez.png"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "dez.png"),
            Path.Combine(Environment.CurrentDirectory, "Assets", "dez.png")
        };

        var iconPath = iconPathCandidates.FirstOrDefault(File.Exists);
        if (!string.IsNullOrWhiteSpace(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }
    }

    private void ApplyTitleBarColors()
    {
        if (!AppWindowTitleBar.IsCustomizationSupported())
        {
            return;
        }

        var titleBar = AppWindow.TitleBar;

        var isDark = RootGrid.ActualTheme == ElementTheme.Dark;
        var backgroundColor = isDark
            ? Color.FromArgb(255, 32, 32, 32)
            : Color.FromArgb(255, 243, 243, 243);
        var foregroundColor = isDark ? Colors.White : Colors.Black;
        var hoverBackgroundColor = isDark
            ? Color.FromArgb(255, 58, 58, 58)
            : Color.FromArgb(255, 232, 232, 232);
        var pressedBackgroundColor = isDark
            ? Color.FromArgb(255, 72, 72, 72)
            : Color.FromArgb(255, 215, 215, 215);

        titleBar.BackgroundColor = backgroundColor;
        titleBar.ForegroundColor = foregroundColor;
        titleBar.InactiveBackgroundColor = backgroundColor;
        titleBar.InactiveForegroundColor = foregroundColor;

        titleBar.ButtonBackgroundColor = backgroundColor;
        titleBar.ButtonForegroundColor = foregroundColor;
        titleBar.ButtonHoverBackgroundColor = hoverBackgroundColor;
        titleBar.ButtonHoverForegroundColor = foregroundColor;
        titleBar.ButtonPressedBackgroundColor = pressedBackgroundColor;
        titleBar.ButtonPressedForegroundColor = foregroundColor;

        titleBar.ButtonInactiveBackgroundColor = backgroundColor;
        titleBar.ButtonInactiveForegroundColor = foregroundColor;
    }

    private void SetDialogServiceXamlRoot()
    {
        var dialogService = App.Services.GetRequiredService<IDialogService>() as DialogService;

        // 2. تنظیم XamlRoot آن
        if (dialogService != null)
        {
            // 'this.Content' به ریشه UI درخت ویژوال پنجره فعلی اشاره دارد
            dialogService.XamlRoot = this.Content.XamlRoot;
        }
    }

    private void Instance_Loaded(object sender, RoutedEventArgs e)
    {
        ShellPage.Instance.Loaded -= Instance_Loaded;

        SetDialogServiceXamlRoot();

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
                    ShellPage.Instance.Navigate(typeof(AssessmentReportPage));
                    break;
                case "UserManagement":
                    ShellPage.Instance.Navigate(typeof(UserManagementPage));
                    break;
            }
        }
    }

    private void LogoutNavItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.LogoutCommand.Execute(e);
    }
}