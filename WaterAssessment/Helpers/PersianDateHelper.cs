using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Helpers
{
    public class PersianDateHelper
    {
        private static readonly PersianCalendar _pc = new PersianCalendar();

        // خروجی: 1402/05/12
        public static string ToShamsi(DateTime date)
        {
            if (date is DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    return string.Empty;
                }
                return $"{_pc.GetYear(date)}/{_pc.GetMonth(date):00}/{_pc.GetDayOfMonth(date):00}";
            }

            return string.Empty;
        }

        // اگر DateTime? داشتی
        public static string ToShamsi(DateTime? date)
        {
            if (!date.HasValue)
                return string.Empty;

            return ToShamsi(date.Value);
        }
    }
}
