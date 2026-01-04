namespace WaterAssessment.Converters
{
    public class NullableDoubleToDoubleConverter : IValueConverter
    {
        // از ViewModel به UI (تبدیل null به NaN)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                return d;
            }
            // اگر نال بود، NaN برگردان تا نامبرباکس خالی بماند
            return double.NaN;
        }

        // از UI به ViewModel (تبدیل NaN به null)
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                // اگر کاربر باکس را خالی کرد یا مقدار نامعتبر بود
                if (double.IsNaN(d))
                {
                    return null;
                }
                return d;
            }
            return null;
        }
    }
}
