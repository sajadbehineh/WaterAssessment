namespace WaterAssessment.Models;

public class Location
{
    public int LocationID { get; set; }
    public string Place { get; set; }

    public int AreaID { get; set; }
    public Area Area { get; set; }

    public List<Assessment> Assessments { get; set; }
}