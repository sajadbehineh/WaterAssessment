namespace WaterAssessment.Models;

public class Location
{
    public int LocationID { get; set; }
    public string LocationName { get; set; }

    public int LocationTypeID { get; set; }

    public virtual LocationType LocationType { get; set; }

    public int? GateCount { get; set; }
    public int? PumpCount { get; set; }
    public bool HasPump { get; set; }
    public bool HasGate { get; set; }
    public MeasurementFormType MeasurementFormType { get; set; } = MeasurementFormType.HydrometrySingleSection;
    public int? SectionCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; } // کلید خارجی
    public virtual User UpdatedBy { get; set; }

    public int AreaID { get; set; }
    public virtual Area Area { get; set; }

    public virtual List<LocationPump> LocationPumps { get; set; } = new();
    public virtual List<HydraulicGate> HydraulicGates { get; set; } = new();
    public virtual List<Assessment> Assessments { get; set; } = new();
}