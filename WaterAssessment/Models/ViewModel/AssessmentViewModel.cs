using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace WaterAssessment.Models.ViewModel
{
    public class AssessmentItem
    {
        public int AssessmentID { get; set; }
        public int Timer { get; set; }
        public DateTime Date { get; set; }
        public DateTime Inserted { get; set; }
        public string Echelon { get; set; }
        public string Openness { get; set; }
        public double TotalFlow { get; set; }
        public bool IsCanal { get; set; } = true;
        public int LocationID { get; set; }
        public int CurrentMeterID { get; set; }
        public int PropellerID { get; set; }

        public string Location { get; set; }
        public string CurrentMeterName { get; set; }
        public string PropellerName { get; set; }
        public override string ToString() => $"{CurrentMeterName}-{PropellerName}";

        public int[] FormValueID { get; set; }
        public int[] EmployeeID { get; set; }

        public string Employee_1 { get; set; }
        public string Employee_2 { get; set; }
        public string Employee_3 { get; set; }

        public int RowsCount { get; set; }
    }
}
