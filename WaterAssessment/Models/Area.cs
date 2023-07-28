using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models
{
    public class Area
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        public List<Location> Locations { get; set; }
    }
}
