namespace WaterAssessment.Models;

public class GateFlowRow
{
    public int Id { get; set; }
    public int AssessmentID { get; set; }
    public virtual Assessment Assessment { get; set; }
    public int HydraulicGateID { get; set; }
    public virtual HydraulicGate HydraulicGate { get; set; }
    public double OpeningHeight { get; set; }
    public double UpstreamHead { get; set; }
    public double CalculatedFlow { get; set; }
}
