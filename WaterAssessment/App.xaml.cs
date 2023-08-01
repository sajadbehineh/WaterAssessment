namespace WaterAssessment
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            ThemeManager.Initialize(m_window,
                new ThemeOptions
                {
                    BackdropType = BackdropType.Mica,
                    ElementTheme = ElementTheme.Default,
                    TitleBarCustomization = new TitleBarCustomization
                    {
                        TitleBarType = TitleBarType.AppWindow
                    }
                });

            m_window.Activate();
        }

        private Window m_window;
    }
}
