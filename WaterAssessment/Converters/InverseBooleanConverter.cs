namespace WaterAssessment.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // تبدیل True به False و برعکس برای خواندن از ویومدل
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // تبدیل برعکس برای نوشتن در ویومدل (وقتی کاربر کلیک می‌کند)
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return false;
        }
    }
}
