using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Grid;

namespace WaterAssessment.Converters
{
    public class RowIndexConverter : IValueConverter
    {
        public object EmployeesSource { get; set; }
        public int CurrentPage { get; set; } // اضافه شده برای دریافت صفحه فعلی
        public int PageSize { get; set; }    // اضافه شده برای دریافت تعداد آیتم در هر صفحه

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Employee employee && EmployeesSource is IList list)
            {
                int index = list.IndexOf(employee);
                if (index != -1)
                {
                    // محاسبه ردیف: (صفحه قبلی * سایز صفحه) + ایندکس فعلی + 1
                    int rowNumber = ((CurrentPage - 1) * PageSize) + index + 1;
                    return rowNumber.ToString();
                }
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
