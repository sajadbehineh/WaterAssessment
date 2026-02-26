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

        private void OnEnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.LoginCommand.Execute(TxtPassword);
            }
        }

        public void ResetForm()
        {
            TxtPassword.Password = string.Empty;
            ViewModel.Reset();
        }
    }
}
