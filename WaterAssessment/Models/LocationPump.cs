using System.ComponentModel.DataAnnotations;

namespace WaterAssessment.Models
{
    public class LocationPump
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PumpName { get; set; }
        [Required]
        public double NominalFlow { get; set; }

        public int LocationID { get; set; }
        public virtual Location Location { get; set; }
    }
}
