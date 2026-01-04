namespace WaterAssessment.Models
{
    public class Area
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        public virtual List<Location> Locations { get; set; } = new();
    }
}
