namespace WaterAssessment.Models
{
    public class Assessment_Employee
    {
        public int AssessmentID { get; set; }
        public int EmployeeID { get; set; }
        public virtual Assessment Assessment { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
