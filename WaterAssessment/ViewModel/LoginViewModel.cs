using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Core;
using WaterAssessment.Messages;
using User = WaterAssessment.Models.User;

namespace WaterAssessment.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        // پسورد را نمیتوان بایند کرد (بخاطر امنیت PasswordBox)، پارامتر پاس می‌دهیم

        [ObservableProperty] private bool _isErrorVisible;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isLoading;

        // رویدادی برای بستن پنجره لاگین و باز کردن صفحه اصلی
        public event Action? LoginSuccess;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync(object passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(password)) return;

            IsLoading = true;
            IsErrorVisible = false;

            try
            {
                using var db = new WaterAssessmentContext();

                // 1. پیدا کردن کاربر با نام کاربری
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == Username);

                if (user == null)
                {
                    ShowError("نام کاربری یا رمز عبور اشتباه است.");
                    return;
                }

                // 2. بررسی هش پسورد
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isPasswordValid)
                {
                    // 3. ثبت در سشن
                    AppSession.Login(user);

                    WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(user));

                    // 4. اعلام موفقیت به View (برای نویگیشن)
                    LoginSuccess?.Invoke();
                }
                else
                {
                    ShowError("نام کاربری یا رمز عبور اشتباه است.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطای سیستم: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username);
        }

        private void ShowError(string msg)
        {
            ErrorMessage = msg;
            IsErrorVisible = true;
        }

        // یک متد کمکی برای ساخت یوزر اولیه (فقط یکبار استفاده کنید)
        public async Task SeedAdminAsync()
        {
            using var db = new WaterAssessmentContext();
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    FullName = "مدیر سیستم",
                    // رمز عبور: 123456
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "Admin"
                });
                await db.SaveChangesAsync();
            }
        }
    }
}