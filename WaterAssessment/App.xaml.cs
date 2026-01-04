namespace WaterAssessment
{
    public partial class App : Application
    {
        public IThemeService themeService { get; set; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            themeService = new ThemeService();
            themeService.Initialize(m_window);
            themeService.ConfigBackdrop(BackdropType.DesktopAcrylic);
            themeService.ConfigElementTheme(ElementTheme.Default);
            themeService.ConfigBackdropFallBackColorForWindow10(Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush);

            m_window.Activate();
        }

        private Window m_window;
    }
}
