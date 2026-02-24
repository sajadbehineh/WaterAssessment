using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Services;
using WaterAssessment.Views;

namespace WaterAssessment.ViewModel
{
    public partial class AssessmentReportViewModel : PagedViewModelBase<Assessment>
    {
        private readonly IAssessmentReportService _assessmentReportService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Assessment> Assessments => PagedItems;
        public int TotalAssessments => TotalItems;

        // لیست مکان‌ها برای فیلتر
        public ObservableCollection<Location> Locations { get; } = new();

        public ObservableCollection<LocationType> LocationTypes { get; } = new();

        public ObservableCollection<Employee> Employees { get; } = new();

        // =======================
        // فیلترها
        // =======================
        [ObservableProperty] private Location? _filterLocation;
        [ObservableProperty] private LocationType? _filterLocationType;
        [ObservableProperty] private Employee? _filterEmployee;
        [ObservableProperty] private DateTimeOffset? _filterStartDate;
        [ObservableProperty] private DateTimeOffset? _filterEndDate;

        // =======================
        // سازنده
        // =======================
        public AssessmentReportViewModel(IAssessmentReportService assessmentReportService, IDialogService dialogService) : base(pageSize: 10)
        {
            _assessmentReportService = assessmentReportService;
            _dialogService = dialogService;
            _ = LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {

            // 1. پر کردن لیست مکان‌ها (فقط کانال‌ها) برای کمبوباکس فیلتر
            Locations.Clear();
            var locs = await _assessmentReportService.GetLocationsAsync();
            foreach (var l in locs)
            {
                Locations.Add(l);
            }

            LocationTypes.Clear();
            var locTypes = await _assessmentReportService.GetLocationTypesAsync();
            foreach (var locType in locTypes)
            {
                LocationTypes.Add(locType);
            }

            Employees.Clear();
            var employees = await _assessmentReportService.GetEmployeesAsync();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }

            // 2. اعمال فیلترها و جستجو
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            var result = await _assessmentReportService.GetAssessmentsAsync(
                FilterLocation?.LocationID,
                FilterLocationType?.LocationTypeID,
                null,
                FilterStartDate?.DateTime.Date,
                FilterEndDate?.DateTime.Date);

            if (FilterEmployee is not null)
            {
                result = result
                    .Where(a => a.AssessmentEmployees.Any(ae => ae.EmployeeID == FilterEmployee.EmployeeID))
                    .ToList();
            }

            SetItems(result);
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            FilterLocation = null;
            FilterLocationType = null;
            FilterEmployee = null;
            FilterStartDate = null;
            FilterEndDate = null;
            await ApplyFiltersAsync();
        }

        // =======================
        // عملیات (ویرایش و حذف)
        // =======================

        [RelayCommand]
        private void EditAssessment(Assessment item)
        {
            if (item == null) return;
            ShellPage.Instance.Navigate(typeof(AssessmentFormPage), null, item);
        }

        [RelayCommand]
        private async Task DeleteAssessment(Assessment item)
        {
            if (item == null) return;

            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "حذف رکورد",
                content: $"آیا از حذف اندازه گیری مربوط به '{item.Location?.LocationName}' مطمئن هستید؟",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "خیر"
            );

            if (confirmed)
            {
                var success = await _assessmentReportService.DeleteAssessmentAsync(item.AssessmentID);
                if (success)
                {
                    SetItems(GetAllItems().Where(assessment => assessment.AssessmentID != item.AssessmentID));
                }
            }
        }
    }
}