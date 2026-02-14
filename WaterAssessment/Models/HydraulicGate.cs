namespace WaterAssessment.Models;

public class HydraulicGate
{
    public int Id { get; set; }
    public int LocationID { get; set; }
    public virtual Location Location { get; set; }
    public int GateNumber { get; set; }
    public double DischargeCoefficient { get; set; }
    public double Width { get; set; }
}

