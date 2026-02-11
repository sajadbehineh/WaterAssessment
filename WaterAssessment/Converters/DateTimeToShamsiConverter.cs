using System.Globalization;

namespace WaterAssessment.Converters
{
    public class DateTimeToShamsiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
            {
                var pc = new PersianCalendar();
                // فرمت خروجی: 1402/05/12
                return $"{pc.GetYear(date)}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
