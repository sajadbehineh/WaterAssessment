using WaterAssessment.Models;

namespace WaterAssessment.Converters;

internal class RadianToVelocityConverter : IValueConverter
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
            var oneSecValue = System.Convert.ToDouble(val);
            if (oneSecValue == 0)
            {
                return oneSecValue;
            }
            if (parameter != null && parameter.GetType() == typeof(Propeller))
            {
                var propeller = (Propeller)parameter;
                double a = propeller.AValue;
                double b = propeller.BValue;
                double v = oneSecValue * a + b;
                return v.ToString("0.###");
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}