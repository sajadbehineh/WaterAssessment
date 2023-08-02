namespace WaterAssessment.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        App.Current.ThemeService.SetThemeRadioButtonDefaultItem(themePanel);
    }

    private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        App.Current.ThemeService.OnThemeRadioButtonChecked(sender);
    }

    private async void BtnSysColorSettings_OnClick(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
    }


}