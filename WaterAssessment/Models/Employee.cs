namespace WaterAssessment.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public override string ToString() => $"{FirstName} {LastName}";

        public List<Assessment_Employee> AssessmentEmployees { get; set; }
    }
}
