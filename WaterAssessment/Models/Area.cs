namespace WaterAssessment.Models
{
    public class Area
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserID { get; set; }
        public virtual User CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserID { get; set; } // کلید خارجی
        public virtual User UpdatedBy { get; set; }
        public virtual List<Location> Locations { get; set; } = new();
    }
}
