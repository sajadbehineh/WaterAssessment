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

        // =======================
        // فیلترها
        // =======================
        [ObservableProperty] private Location _filterLocation;
        [ObservableProperty] private DateTimeOffset? _filterStartDate;
        [ObservableProperty] private DateTimeOffset? _filterEndDate;

        // =======================
        // سازنده
        // =======================
        public ChannelsReportViewModel()
        {
            LoadData();
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(MainWindow.Instance);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        public void LoadData()
        {
            using var db = new WaterAssessmentContext();

            // 1. پر کردن لیست مکان‌ها (فقط کانال‌ها) برای کمبوباکس فیلتر
            Locations.Clear();
            var locs = db.Locations.Where(l => l.IsCanal).ToList();
            foreach (var l in locs) Locations.Add(l);

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
                .Include(a=>a.Propeller)
                .Include(a=>a.AssessmentEmployees)
                .Include(a => a.Location)
                .Include(a=>a.GateOpenings)
                .Include(a => a.FormValues)
                //.Include(a => a.Propeller)
                .Where(a => a.Location.IsCanal)
                .AsQueryable();

            // فیلتر مکان
            if (FilterLocation != null)
            {
                query = query.Where(a => a.LocationID == FilterLocation.LocationID);
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
                // تا آخر آن روز
                query = query.Where(a => a.Date <= dt);
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
            //await using var db = new WaterAssessmentContext();
            //var item1 = await db.Assessments
            //    .Include(a => a.FormValues)
            //    .Include(a => a.Propeller)
            //    .FirstOrDefaultAsync(x => x.AssessmentID == item.AssessmentID);

            if (item == null) return;
            // نویگیت به صفحه فرم با پاس دادن آیتم برای ویرایش
            // (فرض بر این است که ShellPage دارید که فریم اصلی را دارد)
            //var frame = (App.MainWindow.Content as ShellPage)?.AppFrame;
            ShellPage.Instance.Navigate(typeof(AssessmentFormPage), null, item);
            //frame?.Navigate(typeof(AssessmentFormPage), item);
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