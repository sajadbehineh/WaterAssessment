using CommunityToolkit.Mvvm.ComponentModel;

namespace WaterAssessment.Models
{
    public class Employee : ObservableObject
    {
        private int _employeeId;
        public int EmployeeID
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        private string _firstName;
        public  string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public  string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }   

        public override string ToString() => $"{FirstName} {LastName}";

        public virtual List<Assessment_Employee> AssessmentEmployees { get; set; } = new();
    }
}
