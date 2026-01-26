using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Navigation;
using System.Globalization;
using WaterAssessment.Converters;
using Windows.Globalization.NumberFormatting;

namespace WaterAssessment.Views;

public sealed partial class AssessmentFormPage : Page
{
    public AssessmentViewModel ViewModel { get; set; }
    public AssessmentFormPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// این متد وقتی اجرا می‌شود که به این صفحه نویگیت (هدایت) شوید.
    /// پارامتر e.Parameter حاوی همان آبجکت Assessment است که از دیالوگ فرستادید.
    /// </summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Assessment assessmentModel;

        // بررسی می‌کنیم که آیا پارامتری (مدل برای ویرایش) دریافت شده است؟
        if (e.Parameter is Assessment { AssessmentID: > 0 } receivedAssessment)
        {
            // --- حالت ویرایش ---
            // اگر از صفحه‌ای دیگر (مثل لیست) مدل پاس داده شده باشد
            using var db = new WaterAssessmentContext();
            var fullAssessment = db.Assessments
                .AsNoTracking() // برای جلوگیری از تداخل کانکست‌ها
                .Include(a => a.FormValues)          // لود کردن سطرها (فاصله، عمق، دورها)
                .Include(a => a.AssessmentEmployees).ThenInclude(ae => ae.Employee)
                .Include(a => a.GateOpenings)        // لود کردن دریچه‌ها
                .FirstOrDefault(a => a.AssessmentID == receivedAssessment.AssessmentID);

            // اگر به هر دلیلی پیدا نشد (مثلا حذف شده)، همان قبلی را استفاده کن
            assessmentModel = fullAssessment ?? receivedAssessment;
        }
        else
        {
            // --- حالت ثبت جدید ---
            // اگر پارامتر نال بود (مثلاً مستقیم نویگیت شده)، یک مدل خام می‌سازیم
            assessmentModel = new Assessment();
        }

        ViewModel = new AssessmentViewModel(assessmentModel);

        // اتصال به DataContext برای کارکرد بایندینگ‌ها
        this.DataContext = ViewModel;
    }
}
