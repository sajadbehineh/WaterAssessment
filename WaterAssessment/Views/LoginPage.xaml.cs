// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WaterAssessment.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; } = new();
        public LoginPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
