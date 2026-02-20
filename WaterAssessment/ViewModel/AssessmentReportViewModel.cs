using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Models;
using WaterAssessment.Services;
using WaterAssessment.Views;

namespace WaterAssessment.ViewModel
{
    public partial class AssessmentReportViewModel : ObservableObject
    {
        private readonly IAssessmentReportService _assessmentReportService;
        private readonly IDialogService _dialogService;
        // لیست اصلی که در گرید نمایش داده می‌شود
        public ObservableCollection<Assessment> Assessments { get; } = new();

        // لیست مکان‌ها برای فیلتر
        public ObservableCollection<Location> Locations { get; } = new();

        public ObservableCollection<LocationType> LocationTypes { get; } = new();

        // =======================
        // فیلترها
        // =======================
        [ObservableProperty] private Location _filterLocation;
        [ObservableProperty] private LocationType? _filterLocationType;
        [ObservableProperty] private DateTimeOffset? _filterStartDate;
        [ObservableProperty] private DateTimeOffset? _filterEndDate;

        // =======================
        // سازنده
        // =======================
        public AssessmentReportViewModel(IAssessmentReportService assessmentReportService, IDialogService dialogService)
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

            // 2. اعمال فیلترها و جستجو
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            var result = await _assessmentReportService.GetAssessmentsAsync(
                FilterLocation?.LocationID,
                FilterLocationType?.LocationTypeID,
                FilterStartDate?.DateTime.Date,
                FilterEndDate?.DateTime.Date);

            Assessments.Clear();
            foreach (var item in result) Assessments.Add(item);
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            FilterLocation = null;
            FilterLocationType = null;
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
                    Assessments.Remove(item);
                }
            }
        }
    }
}