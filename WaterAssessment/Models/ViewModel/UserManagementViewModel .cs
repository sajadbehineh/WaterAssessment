using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterAssessment.Core;

namespace WaterAssessment.Models.ViewModel
{
    public partial class UserManagementViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private string _newUsername = string.Empty;

        // از PasswordBox در UI استفاده خواهد شد

        [ObservableProperty]
        private string _newFullName = string.Empty;

        [ObservableProperty]
        private string _selectedRole = "User"; // مقدار پیش‌فرض

        public UserManagementViewModel()
        {
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            using var db = new WaterAssessmentContext();
            var userList = await db.Users.AsNoTracking().ToListAsync();
            Users = new ObservableCollection<User>(userList);
        }

        [RelayCommand]
        private async Task AddUserAsync(object passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as Microsoft.UI.Xaml.Controls.PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(password))
            {
                // می‌توانید یک پیام خطا نمایش دهید
                return;
            }

            using var db = new WaterAssessmentContext();

            // چک کردن عدم وجود نام کاربری تکراری
            if (await db.Users.AnyAsync(u => u.Username == NewUsername))
            {
                // نمایش خطا: نام کاربری تکراری است
                return;
            }

            var newUser = new User
            {
                Username = NewUsername,
                FullName = NewFullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = SelectedRole
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            // پاک کردن فیلدها و بارگذاری مجدد لیست
            NewUsername = string.Empty;
            NewFullName = string.Empty;
            if (passwordBox != null) passwordBox.Password = string.Empty;

            await LoadUsersAsync();
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User? userToDelete)
        {
            if (userToDelete == null || userToDelete.UserID == AppSession.CurrentUser?.UserID)
            {
                // جلوگیری از حذف کاربر فعلی
                return;
            }

            using var db = new WaterAssessmentContext();
            db.Users.Remove(userToDelete);
            await db.SaveChangesAsync();

            // حذف از لیست نمایش داده شده
            Users.Remove(userToDelete);
        }
    }
}
