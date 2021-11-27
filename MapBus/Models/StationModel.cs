using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class StationModel
    {
        public string StationUID { get; set; }

        public string StationName { get; set; }

        public double PositionLat { get; set; }

        public double PositionLng { get; set; }

        public List<RouteStopModel> RouteStops { get; set; }

        public StationModel()
        {
            RouteStops = new List<RouteStopModel>();
        }
    }
}
