using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using WaterAssessment.Models;
using WaterAssessment.Views;
using WinRT.Interop;

namespace WaterAssessment.Models.ViewModel
{
    public partial class ChannelsReportViewModel : ObservableObject
    {
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
        public ChannelsReportViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            using var db = new WaterAssessmentContext();

            // 1. پر کردن لیست مکان‌ها (فقط کانال‌ها) برای کمبوباکس فیلتر
            Locations.Clear();
            var locs = db.Locations.ToList();
            foreach (var l in locs) Locations.Add(l);

            LocationTypes.Clear();
            var locTypes = db.LocationTypes.ToList();
            foreach (var locType in locTypes) LocationTypes.Add(locType);

            // 2. اعمال فیلترها و جستجو
            ApplyFilters();
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            using var db = new WaterAssessmentContext();

            // شروع کوئری: فقط رکوردها مربوط به کانال‌ها را بیاور
            // Include کردن Location برای نمایش نام مکان ضروری است
            var query = db.Assessments
                .Include(a => a.Propeller)
                .Include(a => a.AssessmentEmployees)
                .Include(a => a.Location)
                .ThenInclude(l => l.LocationType)
                .Include(a => a.GateOpenings)
                .Include(a => a.FormValues)
                .AsQueryable();

            // فیلتر مکان
            if (FilterLocation != null)
            {
                query = query.Where(a => a.LocationID == FilterLocation.LocationID);
            }

            if (FilterLocationType != null)
            {
                query = query.Where(a => a.Location.LocationTypeID == FilterLocationType.LocationTypeID);
            }

            // فیلتر تاریخ شروع
            if (FilterStartDate.HasValue)
            {
                var dt = FilterStartDate.Value.DateTime.Date;
                query = query.Where(a => a.Date >= dt);
            }

            // فیلتر تاریخ پایان
            if (FilterEndDate.HasValue)
            {
                var dt = FilterEndDate.Value.DateTime.Date;
                var nextDay = FilterEndDate.Value.DateTime.Date.AddDays(1);
                // تا آخر آن روز
                query = query.Where(a => a.Date < nextDay);
            }

            // مرتب‌سازی بر اساس تاریخ (نزولی)
            var result = query.OrderByDescending(a => a.Date).ToList();

            Assessments.Clear();
            foreach (var item in result) Assessments.Add(item);
        }

        [RelayCommand]
        private void ClearFilters()
        {
            FilterLocation = null;
            FilterLocationType = null;
            FilterStartDate = null;
            FilterEndDate = null;
            ApplyFilters();
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

            // نمایش دیالوگ تایید
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "حذف رکورد",
                Content = $"آیا از حذف اندازه گیری مربوط به '{item.Location?.LocationName}' مطمئن هستید؟",
                PrimaryButtonText = "بله، حذف کن",
                CloseButtonText = "خیر",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.Current.themeService.CurrentWindow.Content.XamlRoot,
                FlowDirection = FlowDirection.RightToLeft
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    using var db = new WaterAssessmentContext();
                    db.Assessments.Remove(item); // EF به صورت Cascade فرزندان را پاک می‌کند
                    await db.SaveChangesAsync();

                    Assessments.Remove(item); // حذف از لیست UI
                }
                catch (Exception ex)
                {
                    // نمایش خطا (می‌توانید از InfoBar استفاده کنید)
                }
            }
        }
    }
}