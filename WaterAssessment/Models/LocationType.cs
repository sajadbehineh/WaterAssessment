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
        public string Title { get; set; } // عنوان: کانال، زهکش، رودخانه و...

        public virtual List<Location> Locations { get; set; } = new();
    }
}
