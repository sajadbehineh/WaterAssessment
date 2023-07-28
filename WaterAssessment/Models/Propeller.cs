namespace WaterAssessment.Models;

public class Propeller
{
    public int PropellerID { get; set; }
    public string DeviceNumber { get; set; }
    public double AValue { get; set; }
    public double BValue { get; set; }

    public List<Assessment> Assessments { get; set; }
}

//[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum Timer
{
    //[Description("Thirty Sec")]
    سی = 30,
    //[Description("Fifty Sec")]
    پنجاه = 50,
    //[Description("Sixty Sec")]
    شصت = 60
}