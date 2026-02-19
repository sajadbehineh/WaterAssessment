using Windows.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WaterAssessment.Services;

namespace WaterAssessment
{
    public partial class App : Application
    {
        private const string ConnectionString = @"Server=(local);Database=WaterAssessmentDB;Trusted_Connection=True;Encrypt=False;";
        public static IServiceProvider Services { get; private set; }
        public IThemeService themeService { get; set; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
            //UnhandledException += App_UnhandledException;
        }

        //private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void ConfigureServices()
        {
            var services = new ServiceCollection();


            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IDialogService, DialogService>();

            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<ICurrentMeterService, CurrentMeterService>();
            services.AddTransient<IPropellerService, PropellerService>();
            services.AddTransient<IAreaService, AreaService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<ILocationTypeService, LocationTypeService>();
            services.AddTransient<IAssessmentService, AssessmentService>();
            services.AddTransient<IAssessmentReportService, AssessmentReportService>();
            services.AddTransient<IFormValueService, FormValueService>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IFormValueViewModelFactory, FormValueViewModelFactory>();

            services.AddTransient<EmployeeViewModel>();
            services.AddTransient<CurrentMeterViewModel>();
            services.AddTransient<PropellerViewModel>();
            services.AddTransient<AreaViewModel>();
            services.AddTransient<LocationViewModel>();
            services.AddTransient<LocationTypeViewModel>();
            services.AddTransient<AssessmentReportViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddDbContextFactory<WaterAssessmentContext>(options =>
                options.UseSqlServer(ConnectionString));

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            themeService = App.Services.GetRequiredService<IThemeService>();
            themeService.Initialize(m_window);
            themeService.ConfigBackdrop(BackdropType.Acrylic);

            var resource = Current.Resources["ApplicationPageBackgroundThemeBrush"];
            if (resource is SolidColorBrush scb)
            {
                themeService.ConfigBackdropFallBackBrushForWindow10(scb);
            }
            else
            {
                // fallback if resource missing or not a SolidColorBrush
                themeService.ConfigBackdropFallBackBrushForWindow10(new SolidColorBrush(Colors.White));
            }

            m_window.Activate();
        }

        private Window m_window;
    }
}
