namespace WaterAssessment.Converters;

public class MeasurementFormTypeToPersianConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not MeasurementFormType formType)
        {
            return string.Empty;
        }

        return formType switch
        {
            MeasurementFormType.HydrometrySingleSection => "هیدرومتری تک‌مقطعی",
            MeasurementFormType.HydrometryMultiSection => "هیدرومتری چندمقطعی",
            MeasurementFormType.ManualTotalFlow => "ورود دستی (دبی کل)",
            MeasurementFormType.GateDischargeEquation => "معادله دریچه: Q = C_d · L · h · √(2gH)",
            _ => formType.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}