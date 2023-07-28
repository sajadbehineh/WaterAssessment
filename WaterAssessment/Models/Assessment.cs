namespace WaterAssessment.Models;

public class Assessment
{
    public int AssessmentID { get; set; }
    public int Timer { get; set; }
    public DateTime Date { get; set; }
    public DateTime Inserted { get; set; }
    public string Echelon { get; set; }
    public string Openness { get; set; }
    public double TotalFlow { get; set; }
    public bool IsCanal { get; set; }

    public int LocationID { get; set; }
    public Location Location { get; set; }

    public int CurrentMeterID { get; set; }
    public CurrentMeter CurrentMeter { get; set; }

    public int PropellerID { get; set; }
    public Propeller Propeller { get; set; }

    public List<FormValue> FormValues { get; set; }

    public List<Assessment_Employee> AssessmentEmployees { get; set; }
}