namespace WaterAssessment.Models;

public class CurrentMeter
{
    public int CurrentMeterID { get; set; }
    public string CurrentMeterName { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; } // کلید خارجی
    public virtual User UpdatedBy { get; set; }

    public virtual List<Assessment> Assessments { get; set; } = new();
}