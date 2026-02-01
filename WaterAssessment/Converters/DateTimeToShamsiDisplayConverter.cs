using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Converters
{
    public class DateTimeToShamsiDisplayConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // بررسی می‌کنیم که آیا مقدار ورودی از نوع DateTime است
            if (value is DateTime dateTime)
            {
                // اگر تاریخ مقدار پیش‌فرض داشت، رشته خالی برمی‌گردانیم
                if (dateTime == DateTime.MinValue)
                {
                    return string.Empty;
                }

                // یک نمونه از تقویم فارسی ایجاد می‌کنیم
                PersianCalendar pc = new PersianCalendar();

                // سال، ماه و روز را از تقویم فارسی استخراج می‌کنیم
                int year = pc.GetYear(dateTime);
                int month = pc.GetMonth(dateTime);
                int day = pc.GetDayOfMonth(dateTime);
                int hour = pc.GetHour(dateTime);
                int minute = pc.GetMinute(dateTime);


                // تاریخ شمسی را به همراه ساعت و دقیقه با فرمت دلخواه برمی‌گردانیم
                return $"{hour:D2}:{minute:D2} - {year}/{month:D2}/{day:D2}";
            }

            // اگر مقدار ورودی تاریخ نبود، چیزی نمایش نمی‌دهیم
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // این متد برای تبدیل برعکس (از شمسی به میلادی) است که در اینجا نیازی به آن نداریم
            throw new NotImplementedException();
        }
    }
}
