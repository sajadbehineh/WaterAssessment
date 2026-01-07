namespace WaterAssessment.Models;

public class Location
{
    public int LocationID { get; set; }
    public string LocationName { get; set; }

    public bool IsCanal { get; set; } = true; // کانال است یا زهکش؟
    public int GateCount { get; set; } = 1;     // تعداد دریچه‌ها (مثلاً کانال W1 مقدارش 2 است)

    public int AreaID { get; set; }
    public virtual Area Area { get; set; }

    public virtual List<Assessment> Assessments { get; set; } = new();
}