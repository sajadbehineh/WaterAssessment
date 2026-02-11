using System.Collections;

namespace WaterAssessment.Converters
{
    public class RowIndexConverter : IValueConverter
    {
        public object? ItemsSource { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || ItemsSource is not IList list || PageSize <= 0 || CurrentPage <= 0)
            {
                return "0";
            }

            int index = list.IndexOf(value);
            if (index == -1)
            {
                return "0";
            }

            int rowNumber = ((CurrentPage - 1) * PageSize) + index + 1;
            return rowNumber.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
