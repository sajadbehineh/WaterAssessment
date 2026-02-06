using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Core;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public ObservableCollection<User> Users { get; } = new();

        public ObservableCollection<string> Roles { get; } = ["Admin", "User"];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _newUsername = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _newFullName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _selectedRole = "User"; // مقدار پیش‌فرض

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private User? _selectedUser;

        [ObservableProperty] private bool _isInfoBarOpen;
        [ObservableProperty] private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;
        [ObservableProperty] private string _infoBarMessage = string.Empty;
        [ObservableProperty] private string _addEditButtonContent = "افزودن کاربر";

        public UserManagementViewModel()
        {
            //_dialogService = dialogService;
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var userList = await db.Users.AsNoTracking().ToListAsync();
                Users.Clear();
                foreach (var user in userList)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync($"خطا در بارگذاری: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private bool CanAddUser(object? passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as Microsoft.UI.Xaml.Controls.PasswordBox;
            var password = passwordBox?.Password;

            return !string.IsNullOrWhiteSpace(NewUsername)
                   && !string.IsNullOrWhiteSpace(NewFullName)
                   && !string.IsNullOrWhiteSpace(SelectedRole)
                   && (SelectedUser != null || !string.IsNullOrWhiteSpace(password));
        }

        [RelayCommand(CanExecute = nameof(CanAddUser))]
        private async Task AddUserAsync(object passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewFullName))
            {
                await ShowMessageAsync("تمام فیلدها باید تکمیل شوند.", InfoBarSeverity.Warning);
                return;
            }

            using var db = new WaterAssessmentContext();

            // بررسی تکراری بودن نام کاربری
            var normalizedUsername = NewUsername.Trim().ToLower();
            var duplicateUsername = await db.Users.AnyAsync(u =>
                u.Username.ToLower() == normalizedUsername &&
                (SelectedUser == null || u.UserID != SelectedUser.UserID));

            if (duplicateUsername)
            {
                await ShowMessageAsync("این نام کاربری قبلاً توسط شخص دیگری انتخاب شده است.", InfoBarSeverity.Error);
                return;
            }

            // ۲. بررسی تکراری بودن نام کامل (برای جلوگیری از ثبت دو اکانت برای یک نفر)
            var normalizedFullName = NewFullName.Trim().ToLower();
            var duplicateFullName = await db.Users.AnyAsync(u =>
                u.FullName.ToLower() == normalizedFullName &&
                (SelectedUser == null || u.UserID != SelectedUser.UserID));

            if (duplicateFullName)
            {
                await ShowMessageAsync("کاربری با این نام و نام خانوادگی قبلاً در سیستم ثبت شده است.", InfoBarSeverity.Warning);
                return;
            }

            if (SelectedUser == null)
            {
                // --- حالت افزودن ---
                var newUser = new User
                {
                    Username = NewUsername,
                    FullName = NewFullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = SelectedRole
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                // به جای لود مجدد کل لیست، فقط آیتم جدید را اضافه می‌کنیم
                Users.Add(newUser);
                ClearForm(passwordBox);
                await ShowMessageAsync("کاربر با موفقیت اضافه شد.", InfoBarSeverity.Success);
            }
            else
            {
                // --- حالت ویرایش ---
                var userToUpdate = await db.Users.FirstOrDefaultAsync(u => u.UserID == SelectedUser.UserID);
                if (userToUpdate == null) return;

                userToUpdate.Username = NewUsername;
                userToUpdate.FullName = NewFullName;
                userToUpdate.Role = SelectedRole;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                }

                await db.SaveChangesAsync();

                // *** اصلاح خطای NotSupportedException ***
                var index = Users.IndexOf(SelectedUser);
                if (index != -1)
                {
                    // دیتاگرید از عملیات Replace (Users[i] = ...) پشتیبانی نمی‌کند.
                    // راه حل: حذف و درج مجدد در همان مکان
                    Users.RemoveAt(index);
                    Users.Insert(index, userToUpdate);

                    // انتخاب مجدد (اختیاری، برای جلوگیری از پرش انتخاب)
                    // SelectedUser = userToUpdate; 
                }
                else
                {
                    // اگر ایندکس پیدا نشد (مثلاً فیلتر فعال است)، فقط رفرش کامل انجام بده
                    await LoadUsersAsync();
                }
                ClearForm(passwordBox);
                await ShowMessageAsync("کاربر با موفقیت ویرایش شد.", InfoBarSeverity.Success);
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User? userToDelete)
        {
            if (userToDelete == null) return;

            // 1. نمایش دیالوگ تایید (UI Improvement)
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "حذف کاربر",
                Content = $"آیا از حذف کاربر «{userToDelete.Username}» اطمینان دارید؟",
                PrimaryButtonText = "بله، حذف کن",
                CloseButtonText = "انصراف",
                DefaultButton = ContentDialogButton.Close,
                FlowDirection = FlowDirection.RightToLeft,
                XamlRoot = App.Current.themeService.CurrentWindow.Content.XamlRoot // دسترسی به XamlRoot اصلی
            };

            var result = await deleteDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            if (userToDelete.UserID == AppSession.CurrentUser?.UserID)
            {
                await ShowMessageAsync("امکان حذف کاربر جاری وجود ندارد.", InfoBarSeverity.Warning);
                return;
            }

            try
            {
                using var db = new WaterAssessmentContext();

                // 2. رفع باگ حذف: استفاده از Stub برای جلوگیری از خطای Tracking
                // این روش بدون نیاز به Select کردن، مستقیماً دستور Delete را صادر می‌کند
                var userStub = new User { UserID = userToDelete.UserID };
                db.Entry(userStub).State = EntityState.Deleted;

                await db.SaveChangesAsync();

                // 3. حذف از لیست UI
                Users.Remove(userToDelete);

                // اگر کاربری که حذف شد در حالت ویرایش بود، فرم را پاک کن
                if (SelectedUser?.UserID == userToDelete.UserID)
                {
                    ClearForm();
                }

                await ShowMessageAsync("کاربر حذف شد.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync($"خطا در حذف: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private void EditUser(User? user)
        {
            SelectedUser = user;
        }

        [RelayCommand]
        private void ClearForm(object? passwordBoxParam = null)
        {
            SelectedUser = null;
            NewUsername = string.Empty;
            NewFullName = string.Empty;
            SelectedRole = "User";
            AddEditButtonContent = "افزودن کاربر";
            IsInfoBarOpen = false;
            InfoBarMessage = string.Empty;

            if (passwordBoxParam is Microsoft.UI.Xaml.Controls.PasswordBox passwordBox)
            {
                passwordBox.Password = string.Empty;
            }
        }

        private async Task ShowMessageAsync(string message, InfoBarSeverity severity, int durationSeconds = 4)
        {
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            IsInfoBarOpen = true;

            await Task.Delay(durationSeconds * 1000);

            if (InfoBarMessage == message)
            {
                IsInfoBarOpen = false;
            }
        }

        partial void OnSelectedUserChanged(User? value)
        {
            if (value == null)
            {
                NewUsername = string.Empty;
                NewFullName = string.Empty;
                SelectedRole = "User";
                AddEditButtonContent = "افزودن کاربر";
                return;
            }

            NewUsername = value.Username;
            NewFullName = value.FullName ?? string.Empty;
            SelectedRole = value.Role;
            AddEditButtonContent = "ویرایش کاربر";
        }
    }
}
