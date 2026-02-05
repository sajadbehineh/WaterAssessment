using System.ComponentModel.DataAnnotations;

namespace WaterAssessment.Models
{
    public class AssessmentPump
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsRunning { get; set; }

        public int AssessmentID { get; set; }
        public virtual Assessment Assessment { get; set; }

        public int LocationPumpID { get; set; }
        public virtual LocationPump LocationPump { get; set; }
    }
}
