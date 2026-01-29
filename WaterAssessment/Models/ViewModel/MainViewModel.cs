using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterAssessment.Messages;

namespace WaterAssessment.Models.ViewModel
{
    public partial class MainViewModel : ObservableObject, IRecipient<LoginSuccessMessage>
    {
        [ObservableProperty]
        private bool _isLoggedIn = false;

        [ObservableProperty]
        private bool _isAdmin = false;

        // این پراپرتی برای مخفی/نمایان کردن منوبار استفاده می‌شود
        [ObservableProperty]
        private Visibility _menuVisibility = Visibility.Collapsed;

        public List<Location> Locations { get; private set; } = new();
        public List<CurrentMeter> CurrentMeters { get; private set; } = new();
        public List<Propeller> Propellers { get; private set; } = new();
        public List<Employee> Employees { get; private set; } = new();

        public MainViewModel()
        {
            WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this);
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
        public void Receive(LoginSuccessMessage message)
        {
            var user = message.Value;

            // تغییر وضعیت به لاگین شده
            IsLoggedIn = true;
            MenuVisibility = Visibility.Visible;

            // چک کردن نقش کاربر
            IsAdmin = user.Role == "Admin";
        }
    }
}
