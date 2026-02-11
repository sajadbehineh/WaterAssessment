using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WaterAssessment.ViewModel
{
    // کلاس کمکی برای نگهداری مقادیر ورودی هر دریچه در UI
    public partial class GateInputViewModel : ObservableObject
    {
        public int GateNumber { get; set; } // چون این عدد معمولاً ثابت است، نیاز به ObservableProperty ندارد

        [ObservableProperty]
        private double _value;
    }

    public partial class CreateAssessmentViewModel : ObservableObject
    {
        // ==========================================
        // لیست‌های مرجع (برای پر کردن ComboBox ها)
        // ==========================================

        public List<Location> Locations { get; }
        public List<CurrentMeter> CurrentMeters { get; }
        public List<Propeller> Propellers { get; }
        public List<Employee> AllEmployees { get; }

        // ==========================================
        // مقادیر انتخاب شده (Inputs)
        // ==========================================

        [ObservableProperty] private DateTimeOffset _date = DateTime.Now;
        [ObservableProperty] private int _timer = 50; // مقدار پیش‌فرض
        [ObservableProperty] private double _echelon;

        // لیست پویا برای ورودی‌های گشودگی (که به UI وصل می‌شود)
        public ObservableCollection<GateInputViewModel> GateInputs { get; } = new();

        [ObservableProperty] private Location _selectedLocation;

        partial void OnSelectedLocationChanged(Location value)
        {
            GateInputs.Clear(); // باکس‌های قبلی را پاک کن

            if (value is { GateCount: > 0 })
            {
                // به تعداد دریچه‌های تعریف شده در Location، باکس ورودی بساز
                for (int i = 1; i <= value.GateCount; i++)
                {
                    GateInputs.Add(new GateInputViewModel { GateNumber = i, Value = 0 });
                }
            }
        }

        [ObservableProperty] private CurrentMeter _selectedCurrentMeter;

        [ObservableProperty] private Propeller _selectedPropeller;

        // برای بخش کارمندان
        [ObservableProperty] private Employee _selectedEmployeeToAdd;
        public ObservableCollection<Employee> SelectedTeam { get; } = new();

        // مدیریت خطا
        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private bool _hasError;

        // نتیجه نهایی (اگر موفقیت آمیز بود پر می‌شود)
        public Assessment ResultAssessment { get; private set; }

        // رویدادی برای بستن دیالوگ از طریق کد (اختیاری)
        public Action<ContentDialogResult> CloseAction { get; set; }

        public CreateAssessmentViewModel(
            List<Location> locations,
            List<CurrentMeter> meters,
            List<Propeller> propellers,
            List<Employee> employees)
        {
            Locations = locations;
            CurrentMeters = meters;
            Propellers = propellers;
            AllEmployees = employees;
        }

        // ==========================================
        // متدهای مدیریت تیم
        // ==========================================

        [RelayCommand]
        private void AddToTeam()
        {
            if (SelectedEmployeeToAdd == null) return;

            if (SelectedTeam.Count >= 3)
            {
                ShowError("حداکثر 3 نفر می‌توانند انتخاب شوند.");
                return;
            }

            if (SelectedTeam.Any(e => e.EmployeeID == SelectedEmployeeToAdd.EmployeeID))
            {
                ShowError("این شخص قبلاً اضافه شده است.");
                return;
            }

            SelectedTeam.Add(SelectedEmployeeToAdd);
            SelectedEmployeeToAdd = null; // پاک کردن انتخاب کامبوباکس
            ClearError();
        }

        [RelayCommand]
        private void RemoveFromTeam(Employee emp)
        {
            if (emp != null)
            {
                SelectedTeam.Remove(emp);
                ClearError();
            }
        }

        // ==========================================
        // تایید نهایی و ساخت فرم
        // ==========================================

        [RelayCommand]
        private void Confirm()
        {
            // 1. اعتبارسنجی
            if (SelectedLocation == null)
            {
                ShowError("لطفاً محل اندازه گیری را انتخاب کنید.");
                return;
            }
            if (SelectedPropeller == null)
            {
                ShowError("لطفاً نوع پروانه را انتخاب کنید.");
                return;
            }
            if (SelectedCurrentMeter == null)
            {
                ShowError("لطفاً نوع مولینه را انتخاب کنید.");
                return;
            }
            if (Echelon <= 0) // فرض بر اینکه اشل باید مثبت باشد
            {
                ShowError("لطفاً مقدار اشل را وارد کنید.");
                return;
            }

            // 2. ساخت آبجکت نهایی
            var newAssessment = new Assessment
            {
                Date = Date.DateTime,
                Timer = Timer,
                Echelon = Echelon,
                GateOpenings = GateInputs.Select(g => new AssessmentGate
                {
                    GateNumber = g.GateNumber,
                    Value = g.Value
                }).ToList(),

                LocationID = SelectedLocation.LocationID,
                Location = SelectedLocation,

                CurrentMeterID = SelectedCurrentMeter.CurrentMeterID,
                CurrentMeter = SelectedCurrentMeter,

                PropellerID = SelectedPropeller.PropellerID,
                Propeller = SelectedPropeller,

                // ساخت لیست اولیه کارمندان
                AssessmentEmployees = SelectedTeam.Select(emp => new Assessment_Employee
                {
                    EmployeeID = emp.EmployeeID,
                    Employee = emp
                }).ToList()
            };

            ResultAssessment = newAssessment;

            // بستن دیالوگ با موفقیت
            CloseAction?.Invoke(ContentDialogResult.Primary);
        }

        private void ShowError(string msg)
        {
            ErrorMessage = msg;
            HasError = true;
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }
    }
}
