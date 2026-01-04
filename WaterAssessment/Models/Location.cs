namespace WaterAssessment.Models;

public class Location
{
    public int LocationID { get; set; }
    public string LocationName { get; set; }

    public int AreaID { get; set; }
    public virtual Area Area { get; set; }

    public virtual List<Assessment> Assessments { get; set; } = new();
}