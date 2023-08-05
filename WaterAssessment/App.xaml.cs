namespace WaterAssessment
{
    public partial class App : Application
    {
        public IThemeService ThemeService { get; set; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            ThemeService = new ThemeService();
            ThemeService.Initialize(m_window);
            ThemeService.ConfigBackdrop(BackdropType.DesktopAcrylic);
            ThemeService.ConfigElementTheme(ElementTheme.Default);
            ThemeService.ConfigBackdropFallBackColorForWindow10(Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush);

            m_window.Activate();
        }

        private Window m_window;
    }
}
