using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public List<Location> Locations { get; private set; } = new();
        public List<CurrentMeter> CurrentMeters { get; private set; } = new();
        public List<Propeller> Propellers { get; private set; } = new();
        public List<Employee> Employees { get; private set; } = new();

        public MainViewModel()
        {
            // در سازنده یا یک متد Initialize، داده‌ها را لود کنید
            _ = LoadBaseDataAsync();
        }

        // متدی برای خواندن اطلاعات پایه از دیتابیس
        public async Task LoadBaseDataAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();

                // استفاده از ToListAsync (اگر EF Core Async دارید) یا ToList
                // بهتر است AsNoTracking باشد چون فقط برای خواندن در کامبوباکس است
                Locations = db.Locations.AsNoTracking().ToList();
                CurrentMeters = db.CurrentMeters.AsNoTracking().ToList();
                Propellers = db.Propellers.AsNoTracking().ToList();
                Employees = db.Employees.AsNoTracking().ToList();
            }
            catch (System.Exception ex)
            {
                // مدیریت خطا (مثلاً لاگ کردن)
            }
        }
    }
}
