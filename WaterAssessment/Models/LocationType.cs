using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models
{
    public class LocationType
    {
        [Key]
        public int LocationTypeID { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserID { get; set; }
        public virtual User CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserID { get; set; } // کلید خارجی
        public virtual User UpdatedBy { get; set; }

        public virtual List<Location> Locations { get; set; } = new();
    }
}
