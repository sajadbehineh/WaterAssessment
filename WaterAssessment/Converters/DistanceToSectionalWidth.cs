namespace WaterAssessment.Converters;

internal class DistanceToSectionalWidth : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not null && parameter is double subtractDistance)
        {
            return subtractDistance;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}