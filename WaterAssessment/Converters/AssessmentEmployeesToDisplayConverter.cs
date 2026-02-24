using System.Collections;

namespace WaterAssessment.Converters;

public class AssessmentEmployeesToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not IEnumerable employees)
        {
            return string.Empty;
        }

        var names = new List<string>();

        foreach (var item in employees)
        {
            if (item is Assessment_Employee assessmentEmployee && assessmentEmployee.Employee != null)
            {
                var lastName = assessmentEmployee.Employee.LastName?.Trim();
                var firstName = assessmentEmployee.Employee.FirstName?.Trim();

                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    names.Add(lastName);
                }
                else if (!string.IsNullOrWhiteSpace(firstName))
                {
                    names.Add(firstName);
                }
            }
        }

        return names.Count > 0 ? string.Join("- ", names) : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}