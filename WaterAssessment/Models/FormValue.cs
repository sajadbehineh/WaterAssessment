namespace WaterAssessment.Models
{
    public class FormValue
    {
        public int FormValueID { get; set; }
        public double Distance { get; set; }
        public double Depth { get; set; }
        public string RadianPerTime_1 { get; set; }
        public string RadianPerTime_2 { get; set; }
        public string RadianPerTime_3 { get; set; }

        public int AssessmentID { get; set; }
        public Assessment Assessment { get; set; }
    }
}
