using Microsoft.UI.Xaml.Data;
using System;

namespace WaterAssessment.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // بررسی اینکه مقدار ورودی عدد است یا خیر
            if (value is double d)
            {
                // خواندن فرمت از پارامتر (اگر پارامتر نال بود، پیش‌فرض N2 را در نظر بگیر)
                // مثال پارامترها: "N0" (بدون اعشار), "N3" (سه رقم), "C2" (پول)
                string format = parameter as string ?? "N0";

                return d.ToString(format);
            }

            // اگر مقدار نال بود یا عدد نبود، رشته خالی یا خط تیره برگردان
            return "0.0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // اگر مقدار ورودی یک رشته است و می‌توان آن را به عدد تبدیل کرد...
            if (value is string str && double.TryParse(str, out double result))
            {
                // ...مقدار عددی را برگردان
                return result;
            }

            // در غیر این صورت (اگر رشته خالی است یا فرمت نامعتبر دارد)، مقدار صفر را برگردان
            // این خط از کرش کردن برنامه جلوگیری می‌کند
            return 0.0;
        }
    }
}