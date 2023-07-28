namespace WaterAssessment.Models
{
    public class FormValue
    {
        public int FormValueID { get; set; }
        public double Distance { get; set; }
        public double Depth { get; set; }
        public int RadianPerTime { get; set; }

        public int AssessmentID { get; set; }
        public Assessment Assessment { get; set; }
    }
}
