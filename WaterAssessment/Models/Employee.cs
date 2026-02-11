using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WaterAssessment.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserID { get; set; }
        public virtual User CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserID { get; set; } // کلید خارجی
        public virtual User UpdatedBy { get; set; }

        public override string ToString() => $"{FirstName} {LastName}";

        public virtual List<Assessment_Employee> AssessmentEmployees { get; set; } = new();
    }
}
