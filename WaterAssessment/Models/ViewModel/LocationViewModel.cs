using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Models.ViewModel
{
    public class LocationItem
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public string LocationArea => $"{LocationName} ({AreaName})";
    }
}
