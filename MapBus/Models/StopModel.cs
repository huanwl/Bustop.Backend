using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class StopModel
    {
        public string StopUID { get; set; }

        public string StopName { get; set; }

        public double PositionLat { get; set; }

        public double PositionLng { get; set; }

        public int StopSequence { get; set; }
    }
}
