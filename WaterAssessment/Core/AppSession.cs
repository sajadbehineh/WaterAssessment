using User = WaterAssessment.Models.User;

namespace WaterAssessment.Core
{
    public static class AppSession
    {
        // ذخیره کل شیء کاربر برای دسترسی به نام، نقش و آی‌دی
        public static User? CurrentUser { get; private set; }

        // یک پراپرتی کمکی برای چک کردن اینکه آیا کسی لاگین هست یا نه
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        public static void Login(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
