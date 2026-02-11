using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Core;
using WaterAssessment.Messages;

namespace WaterAssessment.ViewModel
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

        public MainViewModel()
        {
            WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this);
            //WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (r, m) =>
            //{
            //    IsLoggedIn = true;
            //    IsAdmin = AppSession.IsAdmin;
            //    MenuVisibility = Visibility.Visible;
            //});
        }

        [RelayCommand]
        private void Logout()
        {
            // 1. پاک کردن سشن
            AppSession.Logout();

            // 2. به‌روزرسانی وضعیت UI
            IsLoggedIn = false;
            IsAdmin = false;
            MenuVisibility = Visibility.Collapsed;

            // 3. ارسال پیام برای ناوبری به صفحه لاگین
            WeakReferenceMessenger.Default.Send(new LogoutMessage());
            //if (!this.IsLoggedIn)
            //{
            //    ShellPage.Instance.Navigate(typeof(LoginPage));
            //}
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

        // یک کلاس پیام خالی برای اطلاع‌رسانی خروج
        public sealed class LogoutMessage { }
    }
}
