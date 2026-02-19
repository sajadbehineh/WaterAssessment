namespace WaterAssessment.Converters
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        // تبدیل از ViewModel (DateTime) به View (DateTimeOffset)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
            {
                if (date == DateTime.MinValue) return null;
                return new DateTimeOffset(date);
            }
            return null;
        }

        // تبدیل از View (DateTimeOffset) به ViewModel (DateTime)
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset dto)
            {
                return dto.DateTime;
            }
            // اگر کاربر تاریخ را پاک کرد، چه مقداری برگردد؟ (معمولا DateTime.Now یا MinValue)
            return DateTime.Now;
        }
    }
}
