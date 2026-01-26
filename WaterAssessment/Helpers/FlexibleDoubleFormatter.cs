using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.NumberFormatting;

namespace WaterAssessment.Helpers
{
    public class FlexibleDoubleFormatter : INumberFormatter2, INumberParser
    {
        // مقدار پیش‌فرض: ۱ رقم اعشار
        private int _decimals = 1;

        // این پراپرتی را در XAML مقداردهی می‌کنید
        public int Decimals
        {
            get => _decimals;
            set => _decimals = value;
        }

        // متد فرمت‌دهی: عدد را می‌گیرد و با تعداد اعشار مشخص شده برمی‌گرداند
        public string FormatDouble(double value)
        {
            // استفاده از "F" برای نمایش عدد بدون جداکننده هزارگان (Fixed-point)
            // اگر جداکننده هزارگان (،) می‌خواهید از "N" استفاده کنید
            return value.ToString($"F{Decimals}");
        }

        // متد پارس: رشته را می‌گیرد و به عدد تبدیل می‌کند
        public double? ParseDouble(string text)
        {
            // پاکسازی احتمالی رشته (مثلاً اگر کاربر اشتباهاً کاراکتر خاصی تایپ کرد)
            if (double.TryParse(text, out var dbl))
            {
                // گرد کردن عدد پارس شده به همان تعداد اعشار (اختیاری)
                return Math.Round(dbl, Decimals);
            }
            return null;
        }

        // متدهای الزامی اینترفیس که برای دابل کاربرد ندارند
        public string FormatInt(long value) => throw new NotSupportedException();
        public string FormatUInt(ulong value) => throw new NotSupportedException();
        public long? ParseInt(string text) => throw new NotSupportedException();
        public ulong? ParseUInt(string text) => throw new NotSupportedException();
    }
}
