using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models
{
    public class AssessmentGate
    {
        public int Id { get; set; }
        public int GateNumber { get; set; } // شماره دریچه (1، 2، ...)

        // ذخیره مقدار گشودگی (مثلاً 10.5 درصد)
        public double Value { get; set; }

        public int AssessmentID { get; set; }
        public virtual Assessment Assessment { get; set; }
    }
}
