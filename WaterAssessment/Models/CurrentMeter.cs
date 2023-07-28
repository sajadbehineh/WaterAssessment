namespace WaterAssessment.Models;

public class CurrentMeter
{
    public int CurrentMeterID { get; set; }
    public string CurrentMeterName { get; set; }

    public List<Assessment> Assessments { get; set; }
}