using Microsoft.Extensions.DependencyInjection;
using WaterAssessment.Services;

namespace WaterAssessment
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public IThemeService themeService { get; set; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // --- اینجا تمام سرویس‌ها و ViewModel های خود را ثبت کنید ---

            // ثبت ThemeService به صورت Singleton (یک نمونه برای کل برنامه)
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IDialogService, DialogService>();

            // ثبت EmployeeService به صورت Transient (هر بار درخواست، یک نمونه جدید)
            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<ICurrentMeterService, CurrentMeterService>();
            services.AddTransient<IPropellerService, PropellerService>();
            services.AddTransient<IAreaService, AreaService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<ILocationTypeService, LocationTypeService>();
            services.AddTransient<IAssessmentService, AssessmentService>();

            // ثبت EmployeeViewModel به صورت Transient
            services.AddTransient<EmployeeViewModel>();
            services.AddTransient<CurrentMeterViewModel>();
            services.AddTransient<PropellerViewModel>();
            services.AddTransient<AreaViewModel>();
            services.AddTransient<LocationViewModel>();
            services.AddTransient<LocationTypeViewModel>();
            //services.AddTransient<AssessmentViewModel>();

            // services.AddDbContext<WaterAssessmentContext>(...);

            // ۴. ساخت ServiceProvider
            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            //themeService = new ThemeService();
            themeService = App.Services.GetRequiredService<IThemeService>();
            themeService.Initialize(m_window);
            themeService.ConfigBackdrop(BackdropType.DesktopAcrylic);
            themeService.ConfigElementTheme(ElementTheme.Default);
            themeService.ConfigBackdropFallBackColorForWindow10(Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush);

            m_window.Activate();
        }

        private Window m_window;
    }
}
