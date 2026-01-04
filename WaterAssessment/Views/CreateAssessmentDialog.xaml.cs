
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WaterAssessment.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateAssessmentDialog : ContentDialog
    {
        public CreateAssessmentViewModel ViewModel { get; }
        public CreateAssessmentDialog(
            List<Location> locs,
            List<CurrentMeter> meters,
            List<Propeller> props,
            List<Employee> emps)
        {
            this.InitializeComponent();
            ViewModel = new CreateAssessmentViewModel(locs, meters, props, emps);

            // اتصال متد بستن ویومدل به دیالوگ
            ViewModel.CloseAction = (result) =>
            {
                // چون این متد داخل کامند اجرا می‌شود، باید مطمئن شویم روی Thread اصلی نیستیم یا هندل کنیم
                // اما در ContentDialog معمولا با ست کردن Result بسته می‌شود
                // یک راه ساده تر این است که دکمه Primary را روی Click هندل کنیم 
                // اما چون Command داریم، اینجا فقط Hide میکنیم اگر موفق بود

                if (result == ContentDialogResult.Primary)
                {
                    this.Hide();
                }
            };
        }
    }
}
