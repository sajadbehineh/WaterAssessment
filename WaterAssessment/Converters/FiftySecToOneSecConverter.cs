namespace WaterAssessment.Converters;

internal class ToOneSecConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if ((string)value == string.Empty)
        {
            return null;
        }

        if (value.GetType() == typeof(string))
        {
            var val = (string)value;
            //var valueToDouble = System.Convert.ToDouble(val);
            var valueToDouble = ToDoubleSafeHelper.ToDoubleSafe(val);
            if (parameter != null && parameter.GetType() == typeof(int))
            {
                var t = (int)parameter;
                var timer = System.Convert.ToDouble(t);
                var oneSec = valueToDouble / timer;
                return oneSec.ToString("0.###");
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}