using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WaterAssessment.Models.ViewModel
{
    public partial class CreateAssessmentViewModel:ObservableObject
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
        [ObservableProperty] private double? _openness;
        [ObservableProperty] private bool _isCanal = true; // پیش‌فرض کانال

        [ObservableProperty] private Location _selectedLocation;
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
                Openness = Openness,
                IsCanal = IsCanal,

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
