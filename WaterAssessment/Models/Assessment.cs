namespace WaterAssessment.Models;

public class Assessment
{
    public int AssessmentID { get; set; }
    public int Timer { get; set; }
    public DateTime Date { get; set; }
    public DateTime Inserted { get; set; } = DateTime.Now;
    public double? Echelon { get; set; }
    public double? Openness { get; set; }
    public bool IsCanal { get; set; }
    public double TotalFlow { get; set; }

    public int LocationID { get; set; }
    public virtual Location Location { get; set; }

    public int CurrentMeterID { get; set; }
    public virtual CurrentMeter CurrentMeter { get; set; }

    public int PropellerID { get; set; }
    public virtual Propeller Propeller { get; set; }

    public virtual List<FormValue> FormValues { get; set; } = new();

    public virtual List<Assessment_Employee> AssessmentEmployees { get; set; }=new();
}