using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models
{
    public class Assessment_Employee
    {
        public int AssessmentID { get; set; }
        public int EmployeeID { get; set; }
        public Assessment Assessment { get; set; }
        public Employee Employee { get; set; }
    }
}
